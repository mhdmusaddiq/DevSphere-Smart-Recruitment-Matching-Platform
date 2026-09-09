using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DevSphere.Tests.Security;

public class EmailVerificationChallengeRuntimeTests
{
    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<DevSphereDbContext>(
            options =>
                options.UseInMemoryDatabase(
                    Guid.NewGuid().ToString()));

        services
            .AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<DevSphereDbContext>();

        services.AddScoped<
            IPasswordHasher<ApplicationUser>,
            PasswordHasher<ApplicationUser>>();

        services.AddScoped<
            EmailVerificationChallengeService>();

        return services.BuildServiceProvider();
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        IServiceProvider services,
        string email)
    {
        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = "Runtime Candidate",
            IsActive = true,
            EmailConfirmed = false
        };

        var result =
            await userManager.CreateAsync(
                user,
                "Runtime#Password123");

        Assert.True(
            result.Succeeded,
            string.Join(
                "; ",
                result.Errors.Select(x => x.Description)));

        return user;
    }

    [Fact]
    public async Task Correct_Code_Should_Verify_Email()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "verify@example.com");

        var challenge =
            services.GetRequiredService<
                EmailVerificationChallengeService>();

        var issue =
            await challenge.IssueAsync(user);

        Assert.True(issue.Accepted);
        Assert.False(
            string.IsNullOrWhiteSpace(
                issue.DevelopmentCode));

        var result =
            await challenge.VerifyAsync(
                user,
                issue.DevelopmentCode!);

        Assert.Equal(
            EmailVerificationConsumeResult.Verified,
            result);

        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        var refreshed =
            await userManager.FindByIdAsync(user.Id);

        Assert.NotNull(refreshed);
        Assert.True(refreshed!.EmailConfirmed);
    }

    [Fact]
    public async Task Wrong_Code_Should_Be_Rejected()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "wrong@example.com");

        var challenge =
            services.GetRequiredService<
                EmailVerificationChallengeService>();

        var issue =
            await challenge.IssueAsync(user);

        Assert.True(issue.Accepted);

        var result =
            await challenge.VerifyAsync(
                user,
                "000000");

        Assert.Equal(
            EmailVerificationConsumeResult.Invalid,
            result);

        Assert.False(user.EmailConfirmed);
    }

    [Fact]
    public async Task Five_Wrong_Attempts_Should_Exhaust_Challenge()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "attempts@example.com");

        var challenge =
            services.GetRequiredService<
                EmailVerificationChallengeService>();

        var issue =
            await challenge.IssueAsync(user);

        Assert.True(issue.Accepted);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await challenge.VerifyAsync(
                user,
                "000000");
        }

        var result =
            await challenge.VerifyAsync(
                user,
                issue.DevelopmentCode!);

        Assert.NotEqual(
            EmailVerificationConsumeResult.Verified,
            result);

        Assert.False(user.EmailConfirmed);
    }

    [Fact]
    public async Task Successful_Code_Should_Be_One_Time()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "consume@example.com");

        var challenge =
            services.GetRequiredService<
                EmailVerificationChallengeService>();

        var issue =
            await challenge.IssueAsync(user);

        var first =
            await challenge.VerifyAsync(
                user,
                issue.DevelopmentCode!);

        Assert.Equal(
            EmailVerificationConsumeResult.Verified,
            first);

        var second =
            await challenge.VerifyAsync(
                user,
                issue.DevelopmentCode!);

        Assert.NotEqual(
            EmailVerificationConsumeResult.Verified,
            second);
    }

    [Fact]
    public async Task Immediate_Resend_Should_Be_Blocked_By_Cooldown()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "cooldown@example.com");

        var challenge =
            services.GetRequiredService<
                EmailVerificationChallengeService>();

        var first =
            await challenge.IssueAsync(user);

        Assert.True(first.Accepted);

        var second =
            await challenge.IssueAsync(user);

        Assert.False(second.Accepted);
    }

    [Fact]
    public async Task Resend_Should_Invalidate_Previous_Code()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "resend@example.com");

        var challenge =
            services.GetRequiredService<
                EmailVerificationChallengeService>();

        var first =
            await challenge.IssueAsync(user);

        Assert.True(first.Accepted);
        Assert.NotNull(first.DevelopmentCode);

        var context =
            services.GetRequiredService<
                DevSphereDbContext>();

        var token =
            await context.UserTokens.SingleAsync(
                x =>
                    x.UserId == user.Id &&
                    x.LoginProvider == "DevSphere.Auth" &&
                    x.Name == "EmailVerification");

        Assert.NotNull(token.Value);

        using var document =
            System.Text.Json.JsonDocument.Parse(
                token.Value!);

        var root =
            document.RootElement;

        var state =
            new
            {
                UserId =
                    root.GetProperty("UserId")
                        .GetString(),

                Purpose =
                    root.GetProperty("Purpose")
                        .GetString(),

                NormalizedEmail =
                    root.GetProperty("NormalizedEmail")
                        .GetString(),

                CodeVerifier =
                    root.GetProperty("CodeVerifier")
                        .GetString(),

                IssuedAtUtc =
                    DateTime.UtcNow.AddMinutes(-2),

                ExpiresAtUtc =
                    root.GetProperty("ExpiresAtUtc")
                        .GetDateTime(),

                FailedAttempts =
                    root.GetProperty("FailedAttempts")
                        .GetInt32()
            };

        token.Value =
            System.Text.Json.JsonSerializer.Serialize(
                state);

        await context.SaveChangesAsync();

        var second =
            await challenge.IssueAsync(user);

        Assert.True(second.Accepted);
        Assert.NotNull(second.DevelopmentCode);

        var oldResult =
            await challenge.VerifyAsync(
                user,
                first.DevelopmentCode!);

        Assert.Equal(
            EmailVerificationConsumeResult.Invalid,
            oldResult);

        var newResult =
            await challenge.VerifyAsync(
                user,
                second.DevelopmentCode!);

        Assert.Equal(
            EmailVerificationConsumeResult.Verified,
            newResult);
    }

    [Fact]
    public async Task Sixth_Issue_Within_One_Hour_Should_Be_Blocked()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "rate@example.com");

        var challenge =
            services.GetRequiredService<
                EmailVerificationChallengeService>();

        for (var issueNumber = 1;
             issueNumber <= 5;
             issueNumber++)
        {
            var issue =
                await challenge.IssueAsync(user);

            Assert.True(issue.Accepted);

            var context =
                services.GetRequiredService<
                    DevSphereDbContext>();

            var token =
                await context.UserTokens.SingleAsync(
                    x =>
                        x.UserId == user.Id &&
                        x.LoginProvider ==
                            "DevSphere.Auth" &&
                        x.Name ==
                            "EmailVerification");

            Assert.NotNull(token.Value);

            using var document =
                System.Text.Json.JsonDocument.Parse(
                    token.Value!);

            var root =
                document.RootElement;

            var state =
                new
                {
                    UserId =
                        root.GetProperty("UserId")
                            .GetString(),

                    Purpose =
                        root.GetProperty("Purpose")
                            .GetString(),

                    NormalizedEmail =
                        root.GetProperty(
                            "NormalizedEmail")
                            .GetString(),

                    CodeVerifier =
                        root.GetProperty(
                            "CodeVerifier")
                            .GetString(),

                    IssuedAtUtc =
                        DateTime.UtcNow.AddMinutes(-2),

                    ExpiresAtUtc =
                        root.GetProperty(
                            "ExpiresAtUtc")
                            .GetDateTime(),

                    FailedAttempts =
                        root.GetProperty(
                            "FailedAttempts")
                            .GetInt32()
                };

            token.Value =
                System.Text.Json.JsonSerializer.Serialize(
                    state);

            await context.SaveChangesAsync();
        }

        var sixth =
            await challenge.IssueAsync(user);

        Assert.False(sixth.Accepted);
        Assert.NotNull(sixth.RetryAfter);
        Assert.True(
            sixth.RetryAfter > TimeSpan.Zero);
    }}
