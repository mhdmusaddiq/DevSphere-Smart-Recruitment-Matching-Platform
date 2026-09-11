using Microsoft.AspNetCore.Identity;

namespace DevSphere.Infrastructure.Identity;

public static class RoleSeeder
{
    public static async Task SeedAsync(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        BootstrapAdminOptions? bootstrapAdmin = null)
    {
        string[] roles =
        {
            AppRoles.Admin,
            AppRoles.Employer,
            AppRoles.Candidate
        };

        foreach(var role in roles)
        {
            if(!await roleManager.RoleExistsAsync(role))
            {
                EnsureSucceeded(await roleManager.CreateAsync(
                    new ApplicationRole
                    {
                        Name = role
                    }), $"create the {role} role");
            }
        }

        if (bootstrapAdmin?.Enabled != true)
        {
            return;
        }

        bootstrapAdmin.Validate();
        var adminEmail = bootstrapAdmin.Email.Trim();

        var admin = await userManager
            .FindByEmailAsync(adminEmail);

        if(admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = bootstrapAdmin.DisplayName.Trim(),
                EmailConfirmed = true
            };

            EnsureSucceeded(await userManager.CreateAsync(
                admin,
                bootstrapAdmin.Password), "create the bootstrap administrator");
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
        {
            EnsureSucceeded(await userManager.AddToRoleAsync(
                admin,
                AppRoles.Admin), "assign the bootstrap administrator role");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Unable to {operation}: {errors}");
    }
}

public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";

    public bool Enabled { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = "System Administrator";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(DisplayName))
        {
            throw new InvalidOperationException(
                "BootstrapAdmin is enabled but its runtime configuration is incomplete.");
        }
    }
}
