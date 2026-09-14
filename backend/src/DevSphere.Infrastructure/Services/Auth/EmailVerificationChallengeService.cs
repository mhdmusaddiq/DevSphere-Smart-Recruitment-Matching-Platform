using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using DevSphere.Infrastructure.Configurations;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Services.Auth;

public class EmailVerificationChallengeService
{
    private const string LoginProvider = "DevSphere.Auth";
    private const string TokenName = "EmailVerification";
    private const string RateTokenName = "EmailVerificationRate";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DevSphereDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _hasher;
    private readonly IssueLimitOptions _issueLimits;
    private readonly ConsumeLimitOptions _consumeLimits;

    public EmailVerificationChallengeService(
        UserManager<ApplicationUser> userManager,
        IPasswordHasher<ApplicationUser> hasher,
        DevSphereDbContext context,
        AuthRateLimitOptions rateLimits)
    {
        _userManager = userManager;
        _hasher = hasher;
        _context = context;
        _issueLimits = rateLimits.VerificationIssue;
        _consumeLimits = rateLimits.VerificationConsume;
    }

    public Task<EmailVerificationIssueResult> IssueAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAtomicAsync(
            () => IssueCoreAsync(
                user,
                cancellationToken),
            cancellationToken);
    }

    public Task<EmailVerificationConsumeResult> VerifyAsync(
        ApplicationUser user,
        string code,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAtomicAsync(
            () => VerifyCoreAsync(
                user,
                code,
                cancellationToken),
            cancellationToken);
    }
    private async Task<EmailVerificationIssueResult> IssueCoreAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user.EmailConfirmed)
        {
            return new EmailVerificationIssueResult(
                true,
                null,
                null);
        }

        var now = DateTime.UtcNow;

        var existing = await ReadAsync(
            user,
            cancellationToken);

        if (existing != null &&
            now - existing.IssuedAtUtc < IssueCooldown)
        {
            return new EmailVerificationIssueResult(
                false,
                null,
                IssueCooldown -
                (now - existing.IssuedAtUtc));
        }

        var rateState = await ReadRateStateAsync(
            user,
            cancellationToken);

        var recentIssues = rateState.IssuedAtUtc
            .Where(x => x > now - IssueWindow)
            .OrderBy(x => x)
            .ToList();

        if (recentIssues.Count >= _issueLimits.EmailIssueLimit)
        {
            var retryAfter =
                recentIssues[0] + IssueWindow - now;

            return new EmailVerificationIssueResult(
                false,
                null,
                retryAfter > TimeSpan.Zero
                    ? retryAfter
                    : TimeSpan.Zero);
        }

        recentIssues.Add(now);

        await WriteRateStateAsync(
            user,
            new EmailVerificationRateState
            {
                UserId = user.Id,
                NormalizedEmail = NormalizeEmail(user),
                IssuedAtUtc = recentIssues
            },
            cancellationToken);

        var code =
            RandomNumberGenerator
                .GetInt32(0, 1_000_000)
                .ToString("D6");

        var challenge =
            new EmailVerificationChallengeState
            {
                UserId = user.Id,
                Purpose = TokenName,
                NormalizedEmail = NormalizeEmail(user),
                CodeVerifier =
                    _hasher.HashPassword(user, code),
                IssuedAtUtc = now,
                ExpiresAtUtc = now + ChallengeLifetime,
                FailedAttempts = 0
            };

        await WriteAsync(
            user,
            challenge,
            cancellationToken);

        return new EmailVerificationIssueResult(
            true,
            code,
            null);
    }

    private async Task<EmailVerificationConsumeResult> VerifyCoreAsync(
        ApplicationUser user,
        string code,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user.EmailConfirmed)
        {
            return EmailVerificationConsumeResult.AlreadyVerified;
        }

        var challenge = await ReadAsync(
            user,
            cancellationToken);

        if (challenge == null)
        {
            return EmailVerificationConsumeResult.Invalid;
        }

        if (challenge.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await RemoveAsync(
                user,
                cancellationToken);

            return EmailVerificationConsumeResult.Expired;
        }

        if (challenge.FailedAttempts >=
            _consumeLimits.ChallengeFailedAttempts)
        {
            await RemoveAsync(
                user,
                cancellationToken);

            return EmailVerificationConsumeResult.AttemptsExhausted;
        }

        var normalizedEmail = NormalizeEmail(user);

        if (!string.Equals(
                challenge.UserId,
                user.Id,
                StringComparison.Ordinal) ||
            !string.Equals(
                challenge.Purpose,
                TokenName,
                StringComparison.Ordinal) ||
            !string.Equals(
                challenge.NormalizedEmail,
                normalizedEmail,
                StringComparison.Ordinal))
        {
            await RemoveAsync(
                user,
                cancellationToken);

            return EmailVerificationConsumeResult.Invalid;
        }

        var verification =
            _hasher.VerifyHashedPassword(
                user,
                challenge.CodeVerifier,
                code);

        if (verification ==
            PasswordVerificationResult.Failed)
        {
            challenge.FailedAttempts++;

            if (challenge.FailedAttempts >=
                _consumeLimits.ChallengeFailedAttempts)
            {
                await RemoveAsync(
                    user,
                    cancellationToken);

                return EmailVerificationConsumeResult.AttemptsExhausted;
            }

            await WriteAsync(
                user,
                challenge,
                cancellationToken);

            return EmailVerificationConsumeResult.Invalid;
        }

        user.EmailConfirmed = true;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return EmailVerificationConsumeResult.Invalid;
        }

        await RemoveAsync(
            user,
            cancellationToken);

        return EmailVerificationConsumeResult.Verified;
    }

    private async Task<T> ExecuteAtomicAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // EF InMemory is used by unit tests and has no relational
        // transaction semantics. Production SQL Server uses this
        // Serializable transaction for challenge read-modify-write.
        if (!_context.Database.IsRelational())
        {
            return await action();
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        var result =
            await action();

        await transaction.CommitAsync(
            cancellationToken);

        return result;
    }
    private async Task<EmailVerificationChallengeState?> ReadAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value =
            await _userManager.GetAuthenticationTokenAsync(
                user,
                LoginProvider,
                TokenName);

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize
                <EmailVerificationChallengeState>(value);
        }
        catch (JsonException)
        {
            await RemoveAsync(
                user,
                cancellationToken);

            return null;
        }
    }

    private async Task WriteAsync(
        ApplicationUser user,
        EmailVerificationChallengeState challenge,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value =
            JsonSerializer.Serialize(challenge);

        var result =
            await _userManager.SetAuthenticationTokenAsync(
                user,
                LoginProvider,
                TokenName,
                value);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Email verification challenge could not be stored.");
        }
    }

    private async Task RemoveAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _userManager.RemoveAuthenticationTokenAsync(
            user,
            LoginProvider,
            TokenName);
    }

    private async Task<EmailVerificationRateState> ReadRateStateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value =
            await _userManager.GetAuthenticationTokenAsync(
                user,
                LoginProvider,
                RateTokenName);

        if (string.IsNullOrWhiteSpace(value))
        {
            return new EmailVerificationRateState();
        }

        try
        {
            var state =
                JsonSerializer.Deserialize
                    <EmailVerificationRateState>(value);

            if (state == null ||
                !string.Equals(
                    state.UserId,
                    user.Id,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    state.NormalizedEmail,
                    NormalizeEmail(user),
                    StringComparison.Ordinal))
            {
                return new EmailVerificationRateState();
            }

            return state;
        }
        catch (JsonException)
        {
            return new EmailVerificationRateState();
        }
    }

    private async Task WriteRateStateAsync(
        ApplicationUser user,
        EmailVerificationRateState state,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value = JsonSerializer.Serialize(state);

        var result =
            await _userManager.SetAuthenticationTokenAsync(
                user,
                LoginProvider,
                RateTokenName,
                value);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Email verification rate state could not be stored.");
        }
    }

    private string NormalizeEmail(
        ApplicationUser user)
    {
        return _userManager.NormalizeEmail(
            user.Email ?? string.Empty);
    }

    private TimeSpan ChallengeLifetime => TimeSpan.FromMinutes(
        _consumeLimits.ChallengeLifetimeMinutes);

    private TimeSpan IssueCooldown => TimeSpan.FromSeconds(
        _issueLimits.CooldownSeconds);

    private TimeSpan IssueWindow => TimeSpan.FromMinutes(
        _issueLimits.EmailWindowMinutes);

    private sealed class EmailVerificationChallengeState
    {
        public string UserId { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;

        public string NormalizedEmail { get; set; } = string.Empty;

        public string CodeVerifier { get; set; } = string.Empty;

        public DateTime IssuedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public int FailedAttempts { get; set; }
    }

    private sealed class EmailVerificationRateState
    {
        public string UserId { get; set; } = string.Empty;

        public string NormalizedEmail { get; set; } = string.Empty;

        public List<DateTime> IssuedAtUtc { get; set; } = [];
    }
}

public sealed record EmailVerificationIssueResult(
    bool Accepted,
    string? DevelopmentCode,
    TimeSpan? RetryAfter);

public enum EmailVerificationConsumeResult
{
    Verified,
    AlreadyVerified,
    Invalid,
    Expired,
    AttemptsExhausted
}
