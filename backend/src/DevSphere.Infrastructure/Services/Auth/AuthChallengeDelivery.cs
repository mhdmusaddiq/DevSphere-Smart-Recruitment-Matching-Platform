using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Logging;

namespace DevSphere.Infrastructure.Services.Auth;

public interface IAuthChallengeDelivery
{
    bool IsAvailable { get; }

    Task DeliverAsync(
        string purpose,
        string email,
        string code,
        CancellationToken cancellationToken = default);
}

public sealed class DevelopmentAuthChallengeDelivery
    : IAuthChallengeDelivery
{
    private readonly ILogger<DevelopmentAuthChallengeDelivery> _logger;

    public DevelopmentAuthChallengeDelivery(
        ILogger<DevelopmentAuthChallengeDelivery> logger)
    {
        _logger = logger;
    }

    public bool IsAvailable => true;

    public Task DeliverAsync(
        string purpose,
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "DEVELOPMENT ONLY auth challenge. Purpose: {Purpose}; Email: {Email}; Code: {Code}. No email was sent.",
            purpose,
            email,
            code);

        return Task.CompletedTask;
    }
}

public sealed class UnavailableAuthChallengeDelivery
    : IAuthChallengeDelivery
{
    private readonly ILogger<UnavailableAuthChallengeDelivery> _logger;

    public UnavailableAuthChallengeDelivery(
        ILogger<UnavailableAuthChallengeDelivery> logger)
    {
        _logger = logger;
    }

    public bool IsAvailable => false;

    public Task DeliverAsync(
        string purpose,
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogWarning(
            "Auth challenge delivery is not configured for purpose {Purpose}. No email was sent.",
            purpose);

        return Task.CompletedTask;
    }
}

public sealed class SmtpAuthChallengeOptions
{
    public const string SectionName = "Email:Smtp";

    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "AptLens";

    public bool IsComplete()
    {
        var credentialsAreComplete =
            string.IsNullOrWhiteSpace(Username) ==
            string.IsNullOrWhiteSpace(Password);

        return Enabled &&
            !string.IsNullOrWhiteSpace(Host) &&
            Port is > 0 and <= 65535 &&
            !string.IsNullOrWhiteSpace(FromAddress) &&
            credentialsAreComplete;
    }
}

public sealed record AuthEmailMessage(
    string ToAddress,
    string Subject,
    string TextBody,
    string HtmlBody);

public interface IAuthEmailTransport
{
    Task SendAsync(
        AuthEmailMessage message,
        CancellationToken cancellationToken = default);
}

public sealed class SmtpAuthEmailTransport : IAuthEmailTransport
{
    private readonly SmtpAuthChallengeOptions _options;

    public SmtpAuthEmailTransport(SmtpAuthChallengeOptions options)
    {
        _options = options;
    }

    public async Task SendAsync(
        AuthEmailMessage message,
        CancellationToken cancellationToken = default)
    {
        using var mail = new MailMessage
        {
            From = new MailAddress(
                _options.FromAddress,
                _options.FromName),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true
        };

        mail.To.Add(message.ToAddress);
        mail.AlternateViews.Add(
            AlternateView.CreateAlternateViewFromString(
                message.TextBody,
                null,
                MediaTypeNames.Text.Plain));

        using var client = new SmtpClient(
            _options.Host,
            _options.Port)
        {
            EnableSsl = _options.UseSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            client.Credentials = new NetworkCredential(
                _options.Username,
                _options.Password);
        }

        await client.SendMailAsync(mail, cancellationToken);
    }
}

public sealed class SmtpAuthChallengeDelivery : IAuthChallengeDelivery
{
    private readonly IAuthEmailTransport _transport;
    private readonly ILogger<SmtpAuthChallengeDelivery> _logger;

    public SmtpAuthChallengeDelivery(
        IAuthEmailTransport transport,
        ILogger<SmtpAuthChallengeDelivery> logger)
    {
        _transport = transport;
        _logger = logger;
    }

    public bool IsAvailable => true;

    public async Task DeliverAsync(
        string purpose,
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        var message = Compose(purpose, email, code);

        try
        {
            await _transport.SendAsync(message, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Delivery failure must not change the anti-enumeration response.
            _logger.LogError(
                ex,
                "AptLens auth challenge email delivery failed for purpose {Purpose}.",
                purpose);
        }
    }

    public static AuthEmailMessage Compose(
        string purpose,
        string email,
        string code)
    {
        var isRecovery = purpose.Equals(
            "PasswordRecovery",
            StringComparison.Ordinal);
        var subject = isRecovery
            ? "AptLens password recovery code"
            : "Verify your AptLens email";
        var action = isRecovery
            ? "reset your password"
            : "verify your email";
        var encodedCode = WebUtility.HtmlEncode(code);

        return new AuthEmailMessage(
            email,
            subject,
            $"Use this AptLens code to {action}: {code}. The code expires in 10 minutes. If you did not request this, ignore this email.",
            $"<p>Use this AptLens code to {action}:</p><p><strong>{encodedCode}</strong></p><p>The code expires in 10 minutes. If you did not request this, ignore this email.</p>");
    }
}
