using Xunit;

namespace DevSphere.Tests.Security;

public class AuthChallengeAtomicityContractTests
{
    private static string ReadRepoFile(
        params string[] parts)
    {
        var directory =
            new DirectoryInfo(
                AppContext.BaseDirectory);

        while (directory != null)
        {
            var pathParts =
                new List<string>
                {
                    directory.FullName,
                    "backend"
                };

            pathParts.AddRange(parts);

            var candidate =
                Path.Combine(
                    pathParts.ToArray());

            if (File.Exists(candidate))
            {
                return File.ReadAllText(
                    candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException();
    }

    [Fact]
    public void Email_Verification_Should_Use_Serializable_Transaction()
    {
        var source =
            ReadRepoFile(
                "src",
                "DevSphere.Infrastructure",
                "Services",
                "Auth",
                "EmailVerificationChallengeService.cs");

        Assert.Contains(
            "Database.IsRelational()",
            source);

        Assert.Contains(
            "BeginTransactionAsync",
            source);

        Assert.Contains(
            "IsolationLevel.Serializable",
            source);

        Assert.Contains(
            "CommitAsync",
            source);

        Assert.Contains(
            "IssueCoreAsync",
            source);

        Assert.Contains(
            "VerifyCoreAsync",
            source);
    }

    [Fact]
    public void Password_Recovery_Should_Use_Serializable_Transaction()
    {
        var source =
            ReadRepoFile(
                "src",
                "DevSphere.Infrastructure",
                "Services",
                "Auth",
                "PasswordRecoveryChallengeService.cs");

        Assert.Contains(
            "Database.IsRelational()",
            source);

        Assert.Contains(
            "BeginTransactionAsync",
            source);

        Assert.Contains(
            "IsolationLevel.Serializable",
            source);

        Assert.Contains(
            "CommitAsync",
            source);

        Assert.Contains(
            "IssueCoreAsync",
            source);

        Assert.Contains(
            "ResetCoreAsync",
            source);
    }
}
