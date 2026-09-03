using Microsoft.Extensions.DependencyInjection;

namespace DevSphere.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDevSphereServices(
        this IServiceCollection services)
    {
        return services;
    }
}
