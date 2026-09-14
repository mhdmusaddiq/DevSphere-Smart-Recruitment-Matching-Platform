using DevSphere.Api.Configurations;
using DevSphere.Api.Extensions;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Services.Contacts;
using DevSphere.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthRateLimiting(builder.Configuration);

builder.Services.AddDevSphereServices(builder.Configuration);
builder.Services.AddScoped<IContactRequestService, ContactRequestService>();
builder.Services.AddScoped<EmailVerificationChallengeService>();
builder.Services.AddScoped<PasswordRecoveryChallengeService>();
builder.Services.AddSingleton<IAuthAbuseLimiter, AuthAbuseLimiter>();

var smtpOptions = builder.Configuration
    .GetSection(SmtpAuthChallengeOptions.SectionName)
    .Get<SmtpAuthChallengeOptions>()
    ?? new SmtpAuthChallengeOptions();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<
        IAuthChallengeDelivery,
        DevelopmentAuthChallengeDelivery>();
}
else if (smtpOptions.IsComplete())
{
    builder.Services.AddSingleton(smtpOptions);
    builder.Services.AddScoped<
        IAuthEmailTransport,
        SmtpAuthEmailTransport>();
    builder.Services.AddScoped<
        IAuthChallengeDelivery,
        SmtpAuthChallengeDelivery>();
}
else
{
    builder.Services.AddScoped<
        IAuthChallengeDelivery,
        UnavailableAuthChallengeDelivery>();
}

var app = builder.Build();

if (!app.Environment.IsDevelopment() &&
    !smtpOptions.IsComplete())
{
    app.Logger.LogWarning(
        "SMTP auth challenge delivery is unavailable. Configure Email:Smtp for external users.");
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<ApplicationRole>>();

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();
    var bootstrapAdmin = builder.Configuration
        .GetSection(BootstrapAdminOptions.SectionName)
        .Get<BootstrapAdminOptions>();

    await RoleSeeder.SeedAsync(
        roleManager,
        userManager,
        bootstrapAdmin);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseDevSphereMiddleware();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
