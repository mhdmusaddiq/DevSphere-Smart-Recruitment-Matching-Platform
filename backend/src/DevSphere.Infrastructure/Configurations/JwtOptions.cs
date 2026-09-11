using Microsoft.Extensions.Configuration;

namespace DevSphere.Infrastructure.Configurations;

public sealed class JwtOptions
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; }

    public static JwtOptions LoadRequired(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var expiryIsValid = int.TryParse(
            section[nameof(ExpiryMinutes)],
            out var expiryMinutes);
        var options = new JwtOptions
        {
            SecretKey = section[nameof(SecretKey)] ?? string.Empty,
            Issuer = section[nameof(Issuer)] ?? string.Empty,
            Audience = section[nameof(Audience)] ?? string.Empty,
            ExpiryMinutes = expiryIsValid ? expiryMinutes : 0
        };

        options.Validate();
        return options;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey) ||
            System.Text.Encoding.UTF8.GetByteCount(SecretKey) < 32)
        {
            throw new InvalidOperationException(
                "JwtSettings:SecretKey must be supplied by protected runtime configuration and contain at least 32 bytes.");
        }

        if (string.IsNullOrWhiteSpace(Issuer) ||
            string.IsNullOrWhiteSpace(Audience) ||
            ExpiryMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JwtSettings issuer, audience and positive expiry are required.");
        }
    }
}
