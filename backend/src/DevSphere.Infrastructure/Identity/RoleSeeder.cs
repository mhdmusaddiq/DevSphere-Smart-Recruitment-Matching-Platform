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
                await roleManager.CreateAsync(
                    new ApplicationRole
                    {
                        Name = role
                    });
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

            await userManager.CreateAsync(
                admin,
                "Admin@123");

            await userManager.AddToRoleAsync(
                admin,
                AppRoles.Admin);
        }
    }
}
