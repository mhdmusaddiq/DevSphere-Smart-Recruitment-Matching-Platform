using Xunit;

namespace DevSphere.Tests.Security;

public class AuthChallengeDeliveryContractTests
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
    public void Development_Delivery_Should_Be_Explicitly_NonProduction()
    {
        var source =
            ReadRepoFile(
                "src",
                "DevSphere.Infrastructure",
                "Services",
                "Auth",
                "AuthChallengeDelivery.cs");

        Assert.Contains(
            "DEVELOPMENT ONLY auth challenge",
            source);

        Assert.Contains(
            "No email was sent.",
            source);
    }

    [Fact]
    public void Production_Fallback_Should_Not_Pretend_Email_Was_Sent()
    {
        var source =
            ReadRepoFile(
                "src",
                "DevSphere.Infrastructure",
                "Services",
                "Auth",
                "AuthChallengeDelivery.cs");

        Assert.Contains(
            "Auth challenge delivery is not configured",
            source);

        Assert.Contains(
            "UnavailableAuthChallengeDelivery",
            source);
    }

    [Fact]
    public void Auth_Controller_Should_Use_Delivery_For_Both_Challenges()
    {
        var source =
            ReadRepoFile(
                "src",
                "DevSphere.Api",
                "Controllers",
                "Auth",
                "AuthController.cs");

        Assert.Contains(
            "_challengeDelivery.DeliverAsync",
            source);

        Assert.Contains(
            "\"EmailVerification\"",
            source);

        Assert.Contains(
            "\"PasswordRecovery\"",
            source);
    }

    [Fact]
    public void Program_Should_Select_Delivery_By_Environment()
    {
        var source =
            ReadRepoFile(
                "src",
                "DevSphere.Api",
                "Program.cs");

        Assert.Contains(
            "builder.Environment.IsDevelopment()",
            source);

        Assert.Contains(
            "DevelopmentAuthChallengeDelivery",
            source);

        Assert.Contains(
            "UnavailableAuthChallengeDelivery",
            source);
    }
}
