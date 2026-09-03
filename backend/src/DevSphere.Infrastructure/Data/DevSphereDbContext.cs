using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Data;

public class DevSphereDbContext : DbContext
{
    public DevSphereDbContext(
        DbContextOptions<DevSphereDbContext> options)
        : base(options)
    {
    }
}
