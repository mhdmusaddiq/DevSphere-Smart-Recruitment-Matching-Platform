using DevSphere.Api.Middleware;

namespace DevSphere.Api.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseDevSphereMiddleware(
        this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }
}
