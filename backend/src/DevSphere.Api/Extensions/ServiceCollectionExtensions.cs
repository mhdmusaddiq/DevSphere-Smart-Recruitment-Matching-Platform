using Microsoft.EntityFrameworkCore;
using DevSphere.Infrastructure.Data;

namespace DevSphere.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDevSphereServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DevSphereDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}
