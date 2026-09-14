using System.Globalization;
using System.Threading.RateLimiting;
using DevSphere.Infrastructure.Configurations;
using Microsoft.AspNetCore.RateLimiting;

namespace DevSphere.Api.Extensions;

public static class AuthRateLimitingExtensions
{
    public static IServiceCollection AddAuthRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var limits = configuration
            .GetSection(AuthRateLimitOptions.SectionName)
            .Get<AuthRateLimitOptions>() ?? new AuthRateLimitOptions();
        limits.Validate();
        services.AddSingleton(limits);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter,
                    out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
                            .ToString(CultureInfo.InvariantCulture);
                }

                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        message = "Too many requests. Please try again later."
                    },
                    cancellationToken);
            };

            AddIpPolicy(
                options,
                AuthRateLimitPolicyNames.LoginIp,
                limits.Login.IpPermitLimit,
                limits.Login.IpWindowMinutes);
            AddIpPolicy(
                options,
                AuthRateLimitPolicyNames.VerificationIssueIp,
                limits.VerificationIssue.IpPermitLimit,
                limits.VerificationIssue.IpWindowMinutes);
            AddIpPolicy(
                options,
                AuthRateLimitPolicyNames.VerificationConsumeIp,
                limits.VerificationConsume.IpPermitLimit,
                limits.VerificationConsume.IpWindowMinutes);
            AddIpPolicy(
                options,
                AuthRateLimitPolicyNames.RecoveryIssueIp,
                limits.RecoveryIssue.IpPermitLimit,
                limits.RecoveryIssue.IpWindowMinutes);
            AddIpPolicy(
                options,
                AuthRateLimitPolicyNames.ResetConsumeIp,
                limits.ResetConsume.IpPermitLimit,
                limits.ResetConsume.IpWindowMinutes);
        });

        return services;
    }

    private static void AddIpPolicy(
        RateLimiterOptions options,
        string policyName,
        int permitLimit,
        int windowMinutes)
    {
        options.AddPolicy(policyName, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ??
                    "unknown-remote-address",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromMinutes(windowMinutes),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    }
}
