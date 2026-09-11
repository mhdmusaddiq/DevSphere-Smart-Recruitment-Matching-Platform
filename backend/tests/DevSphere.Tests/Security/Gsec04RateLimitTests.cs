using System.Text.Json;
using DevSphere.Infrastructure.Configurations;
using DevSphere.Infrastructure.Services.Auth;

namespace DevSphere.Tests.Security;

public class Gsec04RateLimitTests
{
    [Fact]
    public void Frozen_Submission_Defaults_Are_Exact_And_Configuration_Backed()
    {
        var options = new AuthRateLimitOptions();
        Assert.Equal(5, options.Login.AccountFailedAttempts);
        Assert.Equal(15, options.Login.AccountWindowMinutes);
        Assert.Equal(30, options.Login.IpPermitLimit);
        Assert.Equal(15, options.Login.IpWindowMinutes);
        AssertIssue(options.VerificationIssue);
        Assert.Equal(30, options.VerificationConsume.IpPermitLimit);
        Assert.Equal(15, options.VerificationConsume.IpWindowMinutes);
        Assert.Equal(5, options.VerificationConsume.ChallengeFailedAttempts);
        Assert.Equal(10, options.VerificationConsume.ChallengeLifetimeMinutes);
        AssertIssue(options.RecoveryIssue);
        Assert.Equal(30, options.ResetConsume.IpPermitLimit);
        Assert.Equal(15, options.ResetConsume.IpWindowMinutes);
        Assert.Equal(5, options.ResetConsume.ChallengeFailedAttempts);
        Assert.Equal(10, options.ResetConsume.ChallengeLifetimeMinutes);

        using var document = JsonDocument.Parse(ReadRepoFile(
            "src", "DevSphere.Api", "appsettings.json"));
        var configured = document.RootElement.GetProperty("AuthRateLimits");
        Assert.Equal(5, configured.GetProperty("Login")
            .GetProperty("AccountFailedAttempts").GetInt32());
        Assert.Equal(30, configured.GetProperty("Login")
            .GetProperty("IpPermitLimit").GetInt32());
        Assert.Equal(20, configured.GetProperty("VerificationIssue")
            .GetProperty("IpPermitLimit").GetInt32());
        Assert.Equal(30, configured.GetProperty("VerificationConsume")
            .GetProperty("IpPermitLimit").GetInt32());
        Assert.Equal(20, configured.GetProperty("RecoveryIssue")
            .GetProperty("IpPermitLimit").GetInt32());
        Assert.Equal(30, configured.GetProperty("ResetConsume")
            .GetProperty("IpPermitLimit").GetInt32());
    }

    [Fact]
    public void Login_Account_Limit_Returns_First_Excess_And_Reset_Clears_State()
    {
        var limiter = new AuthAbuseLimiter();
        const string account = "USER@EXAMPLE.TEST";

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            Assert.True(limiter.RecordLoginFailure(
                account, 5, TimeSpan.FromMinutes(15), out _));
        }

        Assert.False(limiter.RecordLoginFailure(
            account, 5, TimeSpan.FromMinutes(15), out var retryAfter));
        Assert.True(retryAfter > TimeSpan.Zero);

        limiter.ResetLoginFailures(account);
        Assert.True(limiter.RecordLoginFailure(
            account, 5, TimeSpan.FromMinutes(15), out _));
    }

    [Fact]
    public void Endpoint_Policies_Use_RemoteIp_And_Preserve_Generic_Contracts()
    {
        var controller = ReadRepoFile(
            "src", "DevSphere.Api", "Controllers", "Auth",
            "AuthController.cs");
        var policies = ReadRepoFile(
            "src", "DevSphere.Api", "Extensions",
            "AuthRateLimitingExtensions.cs");

        Assert.Contains("AuthRateLimitPolicyNames.LoginIp", controller);
        Assert.Contains("AuthRateLimitPolicyNames.VerificationIssueIp", controller);
        Assert.Contains("AuthRateLimitPolicyNames.VerificationConsumeIp", controller);
        Assert.Contains("AuthRateLimitPolicyNames.RecoveryIssueIp", controller);
        Assert.Contains("AuthRateLimitPolicyNames.ResetConsumeIp", controller);
        Assert.Contains("Connection.RemoteIpAddress", policies);
        Assert.DoesNotContain("X-Forwarded-For", policies);
        Assert.Contains("Status429TooManyRequests", controller);
        Assert.Contains("Status429TooManyRequests", policies);
        Assert.Contains("If the account exists", controller);
        Assert.DoesNotContain("normalizedAccount,", policies);
    }

    [Fact]
    public void Existing_Challenge_Limits_Remain_In_Force()
    {
        foreach (var file in new[]
                 {
                     "EmailVerificationChallengeService.cs",
                     "PasswordRecoveryChallengeService.cs"
                 })
        {
            var source = ReadRepoFile(
                "src", "DevSphere.Infrastructure", "Services", "Auth", file);
            Assert.Contains("ChallengeLifetimeMinutes", source);
            Assert.Contains("CooldownSeconds", source);
            Assert.Contains("EmailWindowMinutes", source);
            Assert.Contains("ChallengeFailedAttempts", source);
            Assert.Contains("EmailIssueLimit", source);
        }
    }

    private static void AssertIssue(IssueLimitOptions options)
    {
        Assert.Equal(5, options.EmailIssueLimit);
        Assert.Equal(60, options.EmailWindowMinutes);
        Assert.Equal(60, options.CooldownSeconds);
        Assert.Equal(20, options.IpPermitLimit);
        Assert.Equal(60, options.IpWindowMinutes);
    }

    private static string ReadRepoFile(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(
                new[] { directory.FullName }.Concat(parts).ToArray());
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }
            directory = directory.Parent;
        }
        throw new FileNotFoundException();
    }
}
