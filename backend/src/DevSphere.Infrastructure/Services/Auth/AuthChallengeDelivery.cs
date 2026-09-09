using Microsoft.Extensions.Logging;

namespace DevSphere.Infrastructure.Services.Auth;

public interface IAuthChallengeDelivery
{
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
