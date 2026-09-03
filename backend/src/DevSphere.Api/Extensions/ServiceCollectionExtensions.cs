using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Services;
using DevSphere.Infrastructure.Services.Profile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

        services.AddScoped<ICandidateProfileService, CandidateProfileService>();

        services.AddScoped<CandidateProfileRepository>();

        services.AddScoped<EmployerProfileRepository>();

        services.AddScoped<VacancyRepository>();

        services.AddScoped<JobApplicationRepository>();

        services.AddScoped<IEmployerProfileService, EmployerProfileService>();

        services.AddScoped<IVacancyService, VacancyService>();

        services.AddScoped<IJobApplicationService, JobApplicationService>();

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
