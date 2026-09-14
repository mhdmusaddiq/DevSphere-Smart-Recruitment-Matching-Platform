using Xunit;

namespace DevSphere.Tests.Security;

public class PasswordRecoveryEndpointContractTests
{
    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(
            AppContext.BaseDirectory);

        while (directory != null)
        {
            var candidate =
                Path.Combine(
                    directory.FullName,
                    relativePath);

            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"Repository file not found: {relativePath}");
    }

    [Fact]
    public void AuthController_Should_Expose_Forgot_And_Reset_Routes()
    {
        var source = ReadRepoFile(
            Path.Combine(
                "backend",
                "src",
                "DevSphere.Api",
                "Controllers",
                "Auth",
                "AuthController.cs"));

        Assert.Contains(
            "[HttpPost(\"forgot-password\")]",
            source);

        Assert.Contains(
            "[HttpPost(\"reset-password\")]",
            source);
    }

    [Fact]
    public void ForgotPassword_Should_Use_Generic_Response_For_Unknown_Email()
    {
        var source = ReadRepoFile(
            Path.Combine(
                "backend",
                "src",
                "DevSphere.Api",
                "Controllers",
                "Auth",
                "AuthController.cs"));

        Assert.Contains(
            "If the account exists, a password recovery challenge has been processed.",
            source);

        Assert.Contains(
            "if (user == null)",
            source);

        Assert.Contains(
            "return Ok(genericResponse);",
            source);
    }

    [Fact]
    public void ForgotPassword_Should_Expose_Code_Only_In_Development()
    {
        var source = ReadRepoFile(
            Path.Combine(
                "backend",
                "src",
                "DevSphere.Api",
                "Controllers",
                "Auth",
                "AuthController.cs"));

        Assert.Contains(
            ".IsDevelopment()",
            source);

        Assert.Contains(
            "developmentCode",
            source);
    }

    [Fact]
    public void ResetPassword_Should_Pass_NewPassword_Without_Trim()
    {
        var source = ReadRepoFile(
            Path.Combine(
                "backend",
                "src",
                "DevSphere.Api",
                "Controllers",
                "Auth",
                "AuthController.cs"));

        Assert.Contains(
            "request.NewPassword",
            source);

        Assert.DoesNotContain(
            "request.NewPassword.Trim",
            source);

        Assert.DoesNotContain(
            "request.NewPassword.ToLower",
            source);

        Assert.DoesNotContain(
            "request.NewPassword.ToUpper",
            source);
    }

    [Fact]
    public void ResetPassword_Should_Use_Generic_Invalid_Challenge_Message()
    {
        var source = ReadRepoFile(
            Path.Combine(
                "backend",
                "src",
                "DevSphere.Api",
                "Controllers",
                "Auth",
                "AuthController.cs"));

        Assert.Contains(
            "The password recovery challenge is invalid or expired.",
            source);

        Assert.Contains(
            "PasswordRecoveryConsumeResult.Reset",
            source);

        Assert.Contains(
            "PasswordRecoveryConsumeResult.PasswordRejected",
            source);
    }

    [Fact]
    public void Program_Should_Register_Both_Auth_Challenge_Services()
    {
        var source = ReadRepoFile(
            Path.Combine(
                "backend",
                "src",
                "DevSphere.Api",
                "Program.cs"));

        Assert.Contains(
            "AddScoped<EmailVerificationChallengeService>()",
            source);

        Assert.Contains(
            "AddScoped<PasswordRecoveryChallengeService>()",
            source);
    }
}
