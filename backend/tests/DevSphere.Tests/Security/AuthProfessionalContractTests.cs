namespace DevSphere.Tests.Security;

public class AuthProfessionalContractTests
{
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(
            AppContext.BaseDirectory);

        while (directory != null)
        {
            var api = Path.Combine(
                directory.FullName,
                "src",
                "DevSphere.Api");

            if (Directory.Exists(api))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Backend repository root was not found.");
    }

    private static string ReadBackendFile(
        params string[] parts)
    {
        var root = RepositoryRoot();

        var pathParts = new List<string>
        {
            root
        };

        pathParts.AddRange(parts);

        return File.ReadAllText(
            Path.Combine(pathParts.ToArray()));
    }

    [Fact]
    public void Logout_Should_Require_Authentication()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Api",
            "Controllers",
            "Auth",
            "AuthController.cs");

        var logoutIndex = source.IndexOf(
            "[HttpPost(\"logout\")]",
            StringComparison.Ordinal);

        Assert.True(logoutIndex >= 0);

        var prefixStart = Math.Max(
            0,
            logoutIndex - 100);

        var prefix = source.Substring(
            prefixStart,
            logoutIndex - prefixStart);

        Assert.Contains(
            "[Authorize]",
            prefix);
    }

    [Fact]
    public void Logout_Should_Use_Authenticated_User()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Api",
            "Controllers",
            "Auth",
            "AuthController.cs");

        Assert.Contains(
            "ClaimTypes.NameIdentifier",
            source);

        Assert.Contains(
            "FindByIdAsync(userId)",
            source);
    }

    [Fact]
    public void Logout_Should_Rotate_Security_Stamp()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Api",
            "Controllers",
            "Auth",
            "AuthController.cs");

        Assert.Contains(
            "UpdateSecurityStampAsync(user)",
            source);

        Assert.Contains(
            "Discard the bearer token",
            source);
    }

    [Fact]
    public void Email_Verification_Should_Use_Secure_Six_Digit_Code()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Auth",
            "EmailVerificationChallengeService.cs");

        Assert.Contains(
            "RandomNumberGenerator",
            source);

        Assert.Contains(
            "GetInt32(0, 1_000_000)",
            source);

        Assert.Contains(
            ".ToString(\"D6\")",
            source);
    }

    [Fact]
    public void Email_Verification_Should_Expire_After_Ten_Minutes()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Auth",
            "EmailVerificationChallengeService.cs");

        Assert.Contains(
            "_consumeLimits.ChallengeLifetimeMinutes",
            source);

        Assert.Contains(
            "ExpiresAtUtc",
            source);
    }

    [Fact]
    public void Email_Verification_Should_Limit_Attempts_To_Five()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Auth",
            "EmailVerificationChallengeService.cs");

        Assert.Contains(
            "_consumeLimits.ChallengeFailedAttempts",
            source);

        Assert.Contains(
            "FailedAttempts++",
            source);

        Assert.Contains(
            "AttemptsExhausted",
            source);
    }

    [Fact]
    public void Email_Verification_Should_Enforce_Resend_Cooldown()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Auth",
            "EmailVerificationChallengeService.cs");

        Assert.Contains(
            "_issueLimits.CooldownSeconds",
            source);

        Assert.Contains(
            "RetryAfter",
            source);
    }

    [Fact]
    public void Email_Verification_Should_Not_Store_Raw_Code()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Auth",
            "EmailVerificationChallengeService.cs");

        Assert.Contains(
            "HashPassword(user, code)",
            source);

        Assert.Contains(
            "CodeVerifier",
            source);

        Assert.Contains(
            "VerifyHashedPassword",
            source);
    }

    [Fact]
    public void Email_Verification_Should_Bind_Email_And_Purpose()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Auth",
            "EmailVerificationChallengeService.cs");

        Assert.Contains(
            "NormalizedEmail",
            source);

        Assert.Contains(
            "Purpose = TokenName",
            source);

        Assert.Contains(
            "string.Equals(",
            source);
    }

    [Fact]
    public void Email_Verification_Should_Consume_Challenge()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Auth",
            "EmailVerificationChallengeService.cs");

        Assert.Contains(
            "RemoveAuthenticationTokenAsync",
            source);

        Assert.Contains(
            "user.EmailConfirmed = true",
            source);
    }
    [Fact]
    public void Email_Verification_Should_Bind_Challenge_To_User_Account()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Auth",
            "EmailVerificationChallengeService.cs");

        Assert.Contains(
            "UserId = user.Id",
            source);

        Assert.Contains(
            "challenge.UserId",
            source);

        Assert.Contains(
            "user.Id",
            source);
    }

    [Fact]
    public void Email_Verification_Should_Limit_Issues_Per_Hour()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Auth",
            "EmailVerificationChallengeService.cs");

        Assert.Contains(
            "_issueLimits.EmailIssueLimit",
            source);

        Assert.Contains(
            "_issueLimits.EmailWindowMinutes",
            source);

        Assert.Contains(
            "RateTokenName",
            source);

        Assert.Contains(
            "recentIssues.Count >= _issueLimits.EmailIssueLimit",
            source);
    }

    [Fact]
    public void Resend_Verification_Should_Use_Generic_Unknown_Email_Response()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Api",
            "Controllers",
            "Auth",
            "AuthController.cs");

        Assert.Contains(
            "[HttpPost(\"resend-verification\")]",
            source);

        Assert.Contains(
            "If the account exists, a verification challenge has been processed.",
            source);

        Assert.Contains(
            "if (user == null)",
            source);

        Assert.Contains(
            "return Ok(genericResponse);",
            source);
    }

    [Fact]
    public void Verification_Code_Should_Only_Be_Exposed_In_Development()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Api",
            "Controllers",
            "Auth",
            "AuthController.cs");

        Assert.Contains(
            ".IsDevelopment()",
            source);

        Assert.Contains(
            "developmentCode",
            source);

        Assert.Contains(
            "issue.DevelopmentCode",
            source);
    }

    [Fact]
    public void Verify_Email_Should_Use_Generic_Invalid_Challenge_Response()
    {
        var source = ReadBackendFile(
            "src",
            "DevSphere.Api",
            "Controllers",
            "Auth",
            "AuthController.cs");

        Assert.Contains(
            "[HttpPost(\"verify-email\")]",
            source);

        Assert.Contains(
            "The verification challenge is invalid or expired.",
            source);

        Assert.Contains(
            "EmailVerificationConsumeResult.Verified",
            source);

        Assert.Contains(
            "EmailVerificationConsumeResult.AlreadyVerified",
            source);
    }}
