using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using DevSphere.Infrastructure.Configurations;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Services.Auth;

public class PasswordRecoveryChallengeService
{
    private const string LoginProvider = "DevSphere.Auth";
    private const string TokenName = "PasswordRecovery";
    private const string RateTokenName = "PasswordRecoveryRate";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DevSphereDbContext _context;
    private readonly IssueLimitOptions _issueLimits;
    private readonly ConsumeLimitOptions _consumeLimits;

    public PasswordRecoveryChallengeService(
        UserManager<ApplicationUser> userManager,
        DevSphereDbContext context,
        AuthRateLimitOptions rateLimits)
    {
        _userManager = userManager;
        _context = context;
        _issueLimits = rateLimits.RecoveryIssue;
        _consumeLimits = rateLimits.ResetConsume;
    }

    public Task<PasswordRecoveryIssueResult> IssueAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAtomicAsync(
            () => IssueCoreAsync(
                user,
                cancellationToken),
            cancellationToken);
    }

    public Task<PasswordRecoveryResetResult> ResetAsync(
        ApplicationUser user,
        string code,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAtomicAsync(
            () => ResetCoreAsync(
                user,
                code,
                newPassword,
                cancellationToken),
            cancellationToken);
    }
    private async Task<PasswordRecoveryIssueResult> IssueCoreAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!user.IsActive)
        {
            return new PasswordRecoveryIssueResult(
                true,
                null,
                null);
        }

        var now = DateTime.UtcNow;

        var existing =
            await ReadAsync(
                user,
                cancellationToken);

        if (existing != null &&
            now - existing.IssuedAtUtc < IssueCooldown)
        {
            return new PasswordRecoveryIssueResult(
                false,
                null,
                IssueCooldown -
                (now - existing.IssuedAtUtc));
        }

        var rateState =
            await ReadRateStateAsync(
                user,
                cancellationToken);

        var recentIssues =
            rateState.IssuedAtUtc
                .Where(x => x > now - IssueWindow)
                .OrderBy(x => x)
                .ToList();

        if (recentIssues.Count >= _issueLimits.EmailIssueLimit)
        {
            var retryAfter =
                recentIssues[0] +
                IssueWindow -
                now;

            return new PasswordRecoveryIssueResult(
                false,
                null,
                retryAfter > TimeSpan.Zero
                    ? retryAfter
                    : TimeSpan.Zero);
        }

        recentIssues.Add(now);

        await WriteRateStateAsync(
            user,
            new PasswordRecoveryRateState
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
            new PasswordRecoveryChallengeState
            {
                UserId = user.Id,
                Purpose = TokenName,
                NormalizedEmail = NormalizeEmail(user),
                CodeVerifier =
                    _userManager.PasswordHasher
                        .HashPassword(user, code),
                IssuedAtUtc = now,
                ExpiresAtUtc = now + ChallengeLifetime,
                FailedAttempts = 0
            };

        await WriteAsync(
            user,
            challenge,
            cancellationToken);

        return new PasswordRecoveryIssueResult(
            true,
            code,
            null);
    }

    private async Task<PasswordRecoveryResetResult> ResetCoreAsync(
        ApplicationUser user,
        string code,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var challenge =
            await ReadAsync(
                user,
                cancellationToken);

        if (challenge == null)
        {
            return Result(
                PasswordRecoveryConsumeResult.Invalid);
        }

        if (challenge.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await RemoveAsync(
                user,
                cancellationToken);

            return Result(
                PasswordRecoveryConsumeResult.Expired);
        }

        if (challenge.FailedAttempts >=
            _consumeLimits.ChallengeFailedAttempts)
        {
            await RemoveAsync(
                user,
                cancellationToken);

            return Result(
                PasswordRecoveryConsumeResult.AttemptsExhausted);
        }

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
                NormalizeEmail(user),
                StringComparison.Ordinal))
        {
            await RemoveAsync(
                user,
                cancellationToken);

            return Result(
                PasswordRecoveryConsumeResult.Invalid);
        }

        var verification =
            _userManager.PasswordHasher
                .VerifyHashedPassword(
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

                return Result(
                    PasswordRecoveryConsumeResult
                        .AttemptsExhausted);
            }

            await WriteAsync(
                user,
                challenge,
                cancellationToken);

            return Result(
                PasswordRecoveryConsumeResult.Invalid);
        }

        var validationErrors =
            new List<string>();

        foreach (var validator
                 in _userManager.PasswordValidators)
        {
            var validation =
                await validator.ValidateAsync(
                    _userManager,
                    user,
                    newPassword);

            if (!validation.Succeeded)
            {
                validationErrors.AddRange(
                    validation.Errors
                        .Select(x => x.Description));
            }
        }

        if (validationErrors.Count > 0)
        {
            return new PasswordRecoveryResetResult(
                PasswordRecoveryConsumeResult.PasswordRejected,
                validationErrors);
        }

        user.PasswordHash =
            _userManager.PasswordHasher
                .HashPassword(
                    user,
                    newPassword);

        var updateResult =
            await _userManager
                .UpdateSecurityStampAsync(user);

        if (!updateResult.Succeeded)
        {
            return new PasswordRecoveryResetResult(
                PasswordRecoveryConsumeResult.Invalid,
                updateResult.Errors
                    .Select(x => x.Description)
                    .ToList());
        }

        await RemoveAsync(
            user,
            cancellationToken);

        return Result(
            PasswordRecoveryConsumeResult.Reset);
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
    private async Task<PasswordRecoveryChallengeState?> ReadAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value =
            await _userManager
                .GetAuthenticationTokenAsync(
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
                <PasswordRecoveryChallengeState>(
                    value);
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
        PasswordRecoveryChallengeState challenge,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value =
            JsonSerializer.Serialize(challenge);

        var result =
            await _userManager
                .SetAuthenticationTokenAsync(
                    user,
                    LoginProvider,
                    TokenName,
                    value);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Password recovery challenge could not be stored.");
        }
    }

    private async Task RemoveAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _userManager
            .RemoveAuthenticationTokenAsync(
                user,
                LoginProvider,
                TokenName);
    }

    private async Task<PasswordRecoveryRateState>
        ReadRateStateAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value =
            await _userManager
                .GetAuthenticationTokenAsync(
                    user,
                    LoginProvider,
                    RateTokenName);

        if (string.IsNullOrWhiteSpace(value))
        {
            return new PasswordRecoveryRateState();
        }

        try
        {
            var state =
                JsonSerializer.Deserialize
                    <PasswordRecoveryRateState>(
                        value);

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
                return new PasswordRecoveryRateState();
            }

            return state;
        }
        catch (JsonException)
        {
            return new PasswordRecoveryRateState();
        }
    }

    private async Task WriteRateStateAsync(
        ApplicationUser user,
        PasswordRecoveryRateState state,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value =
            JsonSerializer.Serialize(state);

        var result =
            await _userManager
                .SetAuthenticationTokenAsync(
                    user,
                    LoginProvider,
                    RateTokenName,
                    value);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Password recovery rate state could not be stored.");
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

    private static PasswordRecoveryResetResult Result(
        PasswordRecoveryConsumeResult status)
    {
        return new PasswordRecoveryResetResult(
            status,
            Array.Empty<string>());
    }

    private sealed class PasswordRecoveryChallengeState
    {
        public string UserId { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;

        public string NormalizedEmail { get; set; } = string.Empty;

        public string CodeVerifier { get; set; } = string.Empty;

        public DateTime IssuedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public int FailedAttempts { get; set; }
    }

    private sealed class PasswordRecoveryRateState
    {
        public string UserId { get; set; } = string.Empty;

        public string NormalizedEmail { get; set; } = string.Empty;

        public List<DateTime> IssuedAtUtc { get; set; } = [];
    }
}

public sealed record PasswordRecoveryIssueResult(
    bool Accepted,
    string? DevelopmentCode,
    TimeSpan? RetryAfter);

public sealed record PasswordRecoveryResetResult(
    PasswordRecoveryConsumeResult Status,
    IReadOnlyList<string> Errors);

public enum PasswordRecoveryConsumeResult
{
    Reset,
    Invalid,
    Expired,
    AttemptsExhausted,
    PasswordRejected
}
