using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Services;

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

        services
            .AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<DevSphereDbContext>()
            ;

        services.AddScoped<TokenService>();

        var jwtSettings = configuration.GetSection("JwtSettings");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtSettings["SecretKey"]!))
                    };
            });

        return services;
    }
}
