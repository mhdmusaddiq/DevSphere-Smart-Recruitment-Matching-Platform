using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DevSphere.Infrastructure.Identity;

namespace DevSphere.Infrastructure.Data;

public class DevSphereDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public DevSphereDbContext(
        DbContextOptions<DevSphereDbContext> options)
        : base(options)
    {
    }
}
