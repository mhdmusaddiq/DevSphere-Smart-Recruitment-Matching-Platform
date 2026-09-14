using DevSphere.Application.DTOs.Employers;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Services.Employers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevSphere.Tests.Employers;

public class CompanyTrustServiceTests
{
    private static DevSphereDbContext CreateContext()
    {
        var options =
            new DbContextOptionsBuilder<DevSphereDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new DevSphereDbContext(options);
    }

    private static CompanyTrustService CreateService(
        DevSphereDbContext context)
    {
        return new CompanyTrustService(
            new CompanyTrustRepository(context));
    }

    [Fact]
    public async Task CreateCompany_CreatesPendingMembershipForAuthenticatedEmployer()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.CreateCompanyAsync(
            "employer-1",
            new CreateCompanyProfileRequest
            {
                Name = "DevSphere Labs",
                Location = "Colombo"
            });

        Assert.Equal(
            CompanyMembershipStatus.Pending.ToString(),
            result.MembershipStatus);

        var membership =
            await context.CompanyMemberships.SingleAsync();

        Assert.Equal("employer-1", membership.EmployerUserId);
        Assert.Equal(result.Id, membership.CompanyId);
    }

    [Fact]
    public async Task GetMine_ReturnsOnlyAuthenticatedEmployerCompanies()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        await service.CreateCompanyAsync(
            "employer-1",
            new CreateCompanyProfileRequest
            {
                Name = "Company One"
            });

        await service.CreateCompanyAsync(
            "employer-2",
            new CreateCompanyProfileRequest
            {
                Name = "Company Two"
            });

        var mine =
            (await service.GetMineAsync("employer-1"))
                .ToList();

        Assert.Single(mine);
        Assert.Equal("Company One", mine[0].Name);
    }

    [Fact]
    public async Task SubmitVerification_ForeignEmployer_IsRejected()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var company = await service.CreateCompanyAsync(
            "employer-1",
            new CreateCompanyProfileRequest
            {
                Name = "Owned Company"
            });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.SubmitVerificationAsync(
                "employer-2",
                company.Id,
                new SubmitCompanyVerificationRequest
                {
                    EvidenceStorageKey = "evidence/file.pdf"
                }));
    }

    [Fact]
    public async Task SubmitVerification_OwnerCreatesPendingReview()
    {
        await using var context = CreateContext();

        context.EmployerProfiles.Add(
            new EmployerProfile
            {
                Id = Guid.NewGuid(),
                UserId = "employer-1",
                CompanyName = "DevSphere Labs",
                ContactEmail = "owner@example.com",
                Website = "https://example.com",
                Location = "Colombo",
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var company = await service.CreateCompanyAsync(
            "employer-1",
            new CreateCompanyProfileRequest
            {
                Name = "DevSphere Labs"
            });

        var result =
            await service.SubmitVerificationAsync(
                "employer-1",
                company.Id,
                new SubmitCompanyVerificationRequest
                {
                    EvidenceStorageKey =
                        "verification/company.pdf",
                    Notes = "Registration evidence"
                });

        Assert.Equal(
            CompanyVerificationStatus.PendingReview.ToString(),
            result.Status);

        var verification =
            await context.CompanyVerifications.SingleAsync();

        Assert.Equal(company.Id, verification.CompanyId);
        Assert.Equal(
            CompanyVerificationStatus.PendingReview.ToString(),
            verification.Status);
    }
}
