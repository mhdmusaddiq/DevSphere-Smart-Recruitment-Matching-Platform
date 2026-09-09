using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Matching;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Services;
using DevSphere.Infrastructure.Services.Profile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DevSphere.Infrastructure.Repositories.Notifications;
using DevSphere.Infrastructure.Repositories.Contacts;
using DevSphere.Infrastructure.Services.Notifications;
using DevSphere.Infrastructure.Configurations;
using DevSphere.Infrastructure.Services.Administration;
using DevSphere.Infrastructure.Services.Applications;
using DevSphere.Infrastructure.Services.Candidates;
using DevSphere.Infrastructure.Services.Dashboards;
using DevSphere.Infrastructure.Services.Employers;
using DevSphere.Infrastructure.Services.Files;
using DevSphere.Infrastructure.Services.Matching;

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
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 15;
            })
            .AddEntityFrameworkStores<DevSphereDbContext>()
            ;

        services.AddScoped<TokenService>();

        services.AddScoped<ICandidateProfileService, CandidateProfileService>();
        services.AddScoped<SkillTaxonomyService>();

        services.AddScoped<CandidateProfileRepository>();

        services.AddScoped<EmployerProfileRepository>();

        services.AddScoped<VacancyRepository>();

        services.AddScoped<JobApplicationRepository>();

        services.AddScoped<IEmployerProfileService, EmployerProfileService>();

        services.AddScoped<IVacancyService, VacancyService>();

        services.AddScoped<IMatchEngine, MatchEngineService>();

        services.AddScoped<IJobApplicationService, JobApplicationService>();

        services.AddScoped<MatchingRepository>();

        services.AddScoped<NotificationRepository>();

        services.AddScoped<ContactRequestRepository>();

        services.AddScoped<INotificationService, NotificationService>();

        services.Configure<FileStorageOptions>(
            configuration.GetSection(FileStorageOptions.SectionName));

        services.AddScoped<ResumeRepository>();
        services.AddScoped<EmployerArchitectureRepository>();
        services.AddScoped<CompanyTrustRepository>();
        services.AddScoped<VacancyPolicyRepository>();
        services.AddScoped<EmployerWorkflowRepository>();
        services.AddScoped<ApplicationHistoryRepository>();
        services.AddScoped<MatchResultRepository>();
        services.AddScoped<AdminRepository>();
        services.AddScoped<DashboardRepository>();
        services.AddScoped<CandidateCareerRepository>();

        services.AddScoped<IResumeService, ResumeService>();
        services.AddScoped<IEmployerArchitectureService, EmployerArchitectureService>();
        services.AddScoped<ICompanyTrustService, CompanyTrustService>();
        services.AddScoped<IVacancyPolicyService, VacancyPolicyService>();
        services.AddScoped<IEmployerWorkflowService, EmployerWorkflowService>();
        services.AddScoped<IApplicationHistoryService, ApplicationHistoryService>();
        services.AddScoped<IMatchResultStore, MatchResultStore>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IFileValidationService, FileValidationService>();
        services.AddScoped<ICandidateCareerService, CandidateCareerService>();

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
