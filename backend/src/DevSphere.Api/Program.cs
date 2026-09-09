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

builder.Services.AddDevSphereServices(builder.Configuration);
builder.Services.AddScoped<IContactRequestService, ContactRequestService>();
builder.Services.AddScoped<EmailVerificationChallengeService>();
builder.Services.AddScoped<PasswordRecoveryChallengeService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<
        IAuthChallengeDelivery,
        DevelopmentAuthChallengeDelivery>();
}
else
{
    builder.Services.AddScoped<
        IAuthChallengeDelivery,
        UnavailableAuthChallengeDelivery>();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<ApplicationRole>>();

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    await RoleSeeder.SeedAsync(
        roleManager,
        userManager);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseDevSphereMiddleware();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
