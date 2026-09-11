using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Configurations;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DevSphere.Tests.Security;

public class PasswordRecoveryChallengeRuntimeTests
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
            PasswordRecoveryChallengeService>();
        services.AddSingleton(new AuthRateLimitOptions());

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
            DisplayName = "Recovery Candidate",
            IsActive = true
        };

        var result =
            await userManager.CreateAsync(
                user,
                "OldPassword#123");

        Assert.True(
            result.Succeeded,
            string.Join(
                "; ",
                result.Errors.Select(
                    x => x.Description)));

        return user;
    }

    [Fact]
    public async Task Valid_Code_Should_Reset_Password_And_Block_Replay()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "reset-success@example.com");

        var recovery =
            services.GetRequiredService<
                PasswordRecoveryChallengeService>();

        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        var issue =
            await recovery.IssueAsync(user);

        Assert.True(issue.Accepted);
        Assert.NotNull(issue.DevelopmentCode);

        var reset =
            await recovery.ResetAsync(
                user,
                issue.DevelopmentCode!,
                "NewPassword#456");

        Assert.Equal(
            PasswordRecoveryConsumeResult.Reset,
            reset.Status);

        Assert.False(
            await userManager.CheckPasswordAsync(
                user,
                "OldPassword#123"));

        Assert.True(
            await userManager.CheckPasswordAsync(
                user,
                "NewPassword#456"));

        var replay =
            await recovery.ResetAsync(
                user,
                issue.DevelopmentCode!,
                "AnotherPassword#789");

        Assert.Equal(
            PasswordRecoveryConsumeResult.Invalid,
            replay.Status);

        Assert.False(
            await userManager.CheckPasswordAsync(
                user,
                "AnotherPassword#789"));
    }

    [Fact]
    public async Task Wrong_Code_Should_Not_Reset_Password()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "reset-wrong@example.com");

        var recovery =
            services.GetRequiredService<
                PasswordRecoveryChallengeService>();

        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        var issue =
            await recovery.IssueAsync(user);

        var wrongCode =
            issue.DevelopmentCode == "000000"
                ? "000001"
                : "000000";

        var result =
            await recovery.ResetAsync(
                user,
                wrongCode,
                "NewPassword#456");

        Assert.Equal(
            PasswordRecoveryConsumeResult.Invalid,
            result.Status);

        Assert.True(
            await userManager.CheckPasswordAsync(
                user,
                "OldPassword#123"));

        Assert.False(
            await userManager.CheckPasswordAsync(
                user,
                "NewPassword#456"));
    }

    [Fact]
    public async Task Expired_Code_Should_Not_Reset_Password()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "reset-expired@example.com");

        var recovery =
            services.GetRequiredService<
                PasswordRecoveryChallengeService>();

        var issue =
            await recovery.IssueAsync(user);

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
                        "PasswordRecovery");

        Assert.NotNull(token.Value);

        using var document =
            System.Text.Json.JsonDocument.Parse(
                token.Value!);

        var root = document.RootElement;

        var expiredState = new
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
                root.GetProperty("IssuedAtUtc")
                    .GetDateTime(),

            ExpiresAtUtc =
                DateTime.UtcNow.AddMinutes(-1),

            FailedAttempts =
                root.GetProperty("FailedAttempts")
                    .GetInt32()
        };

        token.Value =
            System.Text.Json.JsonSerializer.Serialize(
                expiredState);

        await context.SaveChangesAsync();

        var result =
            await recovery.ResetAsync(
                user,
                issue.DevelopmentCode!,
                "NewPassword#456");

        Assert.Equal(
            PasswordRecoveryConsumeResult.Expired,
            result.Status);
    }

    [Fact]
    public async Task Five_Wrong_Attempts_Should_Exhaust_Recovery()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "reset-attempts@example.com");

        var recovery =
            services.GetRequiredService<
                PasswordRecoveryChallengeService>();

        var issue =
            await recovery.IssueAsync(user);

        var wrongCode =
            issue.DevelopmentCode == "000000"
                ? "000001"
                : "000000";

        PasswordRecoveryResetResult? last = null;

        for (var attempt = 0;
             attempt < 5;
             attempt++)
        {
            last =
                await recovery.ResetAsync(
                    user,
                    wrongCode,
                    "NewPassword#456");
        }

        Assert.NotNull(last);

        Assert.Equal(
            PasswordRecoveryConsumeResult
                .AttemptsExhausted,
            last!.Status);

        var correctAfterExhaustion =
            await recovery.ResetAsync(
                user,
                issue.DevelopmentCode!,
                "NewPassword#456");

        Assert.Equal(
            PasswordRecoveryConsumeResult.Invalid,
            correctAfterExhaustion.Status);
    }

    [Fact]
    public async Task Weak_New_Password_Should_Be_Rejected()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "reset-weak@example.com");

        var recovery =
            services.GetRequiredService<
                PasswordRecoveryChallengeService>();

        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        var issue =
            await recovery.IssueAsync(user);

        var result =
            await recovery.ResetAsync(
                user,
                issue.DevelopmentCode!,
                "weak");

        Assert.Equal(
            PasswordRecoveryConsumeResult
                .PasswordRejected,
            result.Status);

        Assert.NotEmpty(result.Errors);

        Assert.True(
            await userManager.CheckPasswordAsync(
                user,
                "OldPassword#123"));
    }

    [Fact]
    public async Task New_Password_Should_Not_Be_Trimmed()
    {
        await using var provider = CreateProvider();

        using var scope = provider.CreateScope();

        var services = scope.ServiceProvider;

        var user =
            await CreateUserAsync(
                services,
                "reset-space@example.com");

        var recovery =
            services.GetRequiredService<
                PasswordRecoveryChallengeService>();

        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        var issue =
            await recovery.IssueAsync(user);

        const string exactPassword =
            "  SpacePassword#456  ";

        var result =
            await recovery.ResetAsync(
                user,
                issue.DevelopmentCode!,
                exactPassword);

        Assert.Equal(
            PasswordRecoveryConsumeResult.Reset,
            result.Status);

        Assert.True(
            await userManager.CheckPasswordAsync(
                user,
                exactPassword));

        Assert.False(
            await userManager.CheckPasswordAsync(
                user,
                "SpacePassword#456"));
    }
}
