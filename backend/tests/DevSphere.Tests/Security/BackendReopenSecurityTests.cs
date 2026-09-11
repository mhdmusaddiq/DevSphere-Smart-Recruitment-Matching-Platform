using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DevSphere.Infrastructure.Configurations;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Services;
using DevSphere.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DevSphere.Tests.Security;

public class BackendReopenSecurityTests
{
    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<DevSphereDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<DevSphereDbContext>();
        services.AddSingleton(new JwtOptions
        {
            SecretKey = "unit-test-only-session-key-32-bytes-minimum",
            Issuer = "DevSphere.Tests",
            Audience = "DevSphere.Tests",
            ExpiryMinutes = 5
        });
        services.AddSingleton(new AuthRateLimitOptions());
        services.AddScoped<TokenService>();
        services.AddScoped<JwtSessionValidator>();
        services.AddScoped<PasswordRecoveryChallengeService>();
        return services.BuildServiceProvider();
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        IServiceProvider services,
        string email)
    {
        var manager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "Session User",
            IsActive = true
        };
        var result = await manager.CreateAsync(user, "OldPassword#123");
        Assert.True(result.Succeeded);
        return user;
    }

    private static async Task<(ApplicationUser User, ClaimsPrincipal Principal)>
        CreateTokenPrincipalAsync(IServiceProvider services, string email)
    {
        var user = await CreateUserAsync(services, email);
        var token = await services.GetRequiredService<TokenService>()
            .CreateToken(user);
        var claims = new JwtSecurityTokenHandler()
            .ReadJwtToken(token)
            .Claims;
        return (user, new ClaimsPrincipal(
            new ClaimsIdentity(claims, "Bearer")));
    }

    [Fact]
    public async Task Disabled_Bootstrap_Does_Not_Create_Administrator()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        await RoleSeeder.SeedAsync(
            services.GetRequiredService<RoleManager<ApplicationRole>>(),
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            new BootstrapAdminOptions { Enabled = false });

        var context = services.GetRequiredService<DevSphereDbContext>();
        Assert.Empty(await context.Users.ToListAsync());
        Assert.Equal(3, await context.Roles.CountAsync());
    }

    [Fact]
    public void Jwt_Runtime_Configuration_Is_Required_And_Validated()
    {
        var missing = new JwtOptions
        {
            Issuer = "issuer",
            Audience = "audience",
            ExpiryMinutes = 60
        };
        Assert.Throws<InvalidOperationException>(() => missing.Validate());

        var valid = new JwtOptions
        {
            SecretKey = "runtime-only-signing-key-at-least-32-bytes",
            Issuer = "issuer",
            Audience = "audience",
            ExpiryMinutes = 60
        };
        valid.Validate();
        Assert.Equal("issuer", valid.Issuer);
    }

    [Fact]
    public async Task Previously_Issued_Token_Is_Rejected_After_Disable()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        var (user, principal) = await CreateTokenPrincipalAsync(
            services, "disabled-session@example.test");
        var validator = services.GetRequiredService<JwtSessionValidator>();
        Assert.True(await validator.IsCurrentAsync(principal));

        user.IsActive = false;
        var result = await services.GetRequiredService<UserManager<ApplicationUser>>()
            .UpdateAsync(user);
        Assert.True(result.Succeeded);
        Assert.False(await validator.IsCurrentAsync(principal));
    }

    [Fact]
    public async Task Previously_Issued_Token_Is_Rejected_After_Logout_Stamp_Change()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        var (user, principal) = await CreateTokenPrincipalAsync(
            services, "logout-session@example.test");
        var validator = services.GetRequiredService<JwtSessionValidator>();
        Assert.True(await validator.IsCurrentAsync(principal));

        var result = await services.GetRequiredService<UserManager<ApplicationUser>>()
            .UpdateSecurityStampAsync(user);
        Assert.True(result.Succeeded);
        Assert.False(await validator.IsCurrentAsync(principal));
    }

    [Fact]
    public async Task Previously_Issued_Token_Is_Rejected_After_Password_Reset()
    {
        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        var (user, principal) = await CreateTokenPrincipalAsync(
            services, "reset-session@example.test");
        var validator = services.GetRequiredService<JwtSessionValidator>();
        Assert.True(await validator.IsCurrentAsync(principal));

        var recovery = services.GetRequiredService<PasswordRecoveryChallengeService>();
        var issue = await recovery.IssueAsync(user);
        var reset = await recovery.ResetAsync(
            user,
            issue.DevelopmentCode!,
            "NewPassword#456");
        Assert.Equal(PasswordRecoveryConsumeResult.Reset, reset.Status);
        Assert.False(await validator.IsCurrentAsync(principal));
    }

    [Fact]
    public void Application_Status_Controller_Uses_Dedicated_Conflict_Contract()
    {
        var source = ReadRepoFile(
            "src", "DevSphere.Api", "Controllers", "Profile",
            "JobApplicationController.cs");
        var dto = ReadRepoFile(
            "src", "DevSphere.Application", "DTOs", "Application",
            "JobApplicationDto.cs");

        Assert.Contains("ApplicationStatusTransitionRequest request", source);
        Assert.DoesNotContain("[FromBody] JobApplicationDto request", source);
        Assert.Contains("catch (KeyNotFoundException", source);
        Assert.Contains("catch (UnauthorizedAccessException", source);
        Assert.Contains("catch (InvalidOperationException", source);
        Assert.Contains("return Conflict", source);
        Assert.Contains("public class ApplicationStatusTransitionRequest", dto);
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
