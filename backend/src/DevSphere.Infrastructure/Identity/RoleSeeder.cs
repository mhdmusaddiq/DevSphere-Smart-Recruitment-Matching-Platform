using Microsoft.AspNetCore.Identity;

namespace DevSphere.Infrastructure.Identity;

public static class RoleSeeder
{
    public static async Task SeedAsync(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
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

        var adminEmail = "admin@devsphere.com";

        var admin = await userManager
            .FindByEmailAsync(adminEmail);

        if(admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "System Administrator",
                EmailConfirmed = true
            };

            EnsureSucceeded(await userManager.CreateAsync(
                admin,
                "Admin@DevSphere2026"), "create the bootstrap administrator");

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
