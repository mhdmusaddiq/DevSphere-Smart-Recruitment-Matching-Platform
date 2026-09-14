using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Services.Administration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevSphere.Tests.Security;

public class AdminCompanyVerificationTests
{
    private static DevSphereDbContext CreateContext()
    {
        var options =
            new DbContextOptionsBuilder<DevSphereDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new DevSphereDbContext(options);
    }

    private static AdminService CreateService(
        DevSphereDbContext context)
    {
        return new AdminService(
            new AdminRepository(context),
            null!);
    }

    private static string ReadRepoFile(
        params string[] parts)
    {
        var directory =
            new DirectoryInfo(
                AppContext.BaseDirectory);

        while (directory != null)
        {
            var pathParts =
                new List<string>
                {
                    directory.FullName,
                    "backend"
                };

            pathParts.AddRange(parts);

            var candidate =
                Path.Combine(
                    pathParts.ToArray());

            if (File.Exists(candidate))
            {
                return File.ReadAllText(
                    candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException();
    }

    [Fact]
    public void Company_Verification_Hooks_Should_Be_Admin_Only()
    {
        var source =
            ReadRepoFile(
                "src",
                "DevSphere.Api",
                "Controllers",
                "Administration",
                "AdminController.cs");

        Assert.Contains(
            "[Authorize(Roles = \"Admin\")]",
            source);

        Assert.Contains(
            "[HttpGet(\"company-verifications\")]",
            source);

        Assert.Contains(
            "[HttpPut(\"company-verifications/{verificationId:guid}/review\")]",
            source);

        Assert.DoesNotContain(
            "matching-score",
            source);

        Assert.DoesNotContain(
            "compatibility-score",
            source);
    }

    [Fact]
    public async Task Default_Queue_Should_Return_Only_PendingReview()
    {
        await using var context =
            CreateContext();

        var company =
            new CompanyProfile
            {
                Id = Guid.NewGuid(),
                Name = "Queue Company"
            };

        context.CompanyProfiles.Add(company);

        context.CompanyVerifications.AddRange(
            new CompanyVerification
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                EmployerProfileId =
                    Guid.NewGuid(),
                Status =
                    CompanyVerificationStatus
                        .PendingReview
                        .ToString(),
                EvidenceStorageKey =
                    "pending.pdf"
            },
            new CompanyVerification
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                EmployerProfileId =
                    Guid.NewGuid(),
                Status =
                    CompanyVerificationStatus
                        .Verified
                        .ToString(),
                EvidenceStorageKey =
                    "verified.pdf"
            });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result =
            await service
                .GetCompanyVerificationsAsync(
                    null);

        Assert.Single(result);

        Assert.Equal(
            CompanyVerificationStatus
                .PendingReview
                .ToString(),
            result[0].Status);

        Assert.Equal(
            "Queue Company",
            result[0].CompanyName);
    }

    [Fact]
    public async Task Admin_Should_Review_Pending_Verification()
    {
        await using var context =
            CreateContext();

        var company =
            new CompanyProfile
            {
                Id = Guid.NewGuid(),
                Name = "Review Company"
            };

        var verification =
            new CompanyVerification
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                EmployerProfileId =
                    Guid.NewGuid(),
                Status =
                    CompanyVerificationStatus
                        .PendingReview
                        .ToString(),
                EvidenceStorageKey =
                    "review.pdf"
            };

        context.CompanyProfiles.Add(company);
        context.CompanyVerifications.Add(
            verification);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result =
            await service
                .ReviewCompanyVerificationAsync(
                    "admin-1",
                    verification.Id,
                    "Verified");

        Assert.NotNull(result);

        Assert.Equal(
            CompanyVerificationStatus
                .Verified
                .ToString(),
            result!.Status);

        var stored =
            await context.CompanyVerifications
                .SingleAsync(
                    x => x.Id ==
                        verification.Id);

        Assert.Equal(
            "admin-1",
            stored.ReviewedByUserId);

        Assert.NotNull(
            stored.ReviewedAtUtc);

        var audit =
            await context.AuditEvents
                .SingleAsync(
                    x =>
                        x.EntityId ==
                        verification.Id
                            .ToString());

        Assert.Equal(
            "CompanyVerificationReviewed",
            audit.Action);
    }

    [Fact]
    public async Task Completed_Verification_Should_Not_Be_Reviewed_Again()
    {
        await using var context =
            CreateContext();

        var verification =
            new CompanyVerification
            {
                Id = Guid.NewGuid(),
                CompanyId = Guid.NewGuid(),
                EmployerProfileId =
                    Guid.NewGuid(),
                Status =
                    CompanyVerificationStatus
                        .Verified
                        .ToString(),
                EvidenceStorageKey =
                    "done.pdf",
                ReviewedAtUtc =
                    DateTime.UtcNow,
                ReviewedByUserId =
                    "admin-old"
            };

        context.CompanyVerifications.Add(
            verification);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<
            InvalidOperationException>(
                () =>
                    service
                        .ReviewCompanyVerificationAsync(
                            "admin-new",
                            verification.Id,
                            "Rejected"));
    }

    [Fact]
    public async Task Invalid_Review_Decision_Should_Be_Rejected()
    {
        await using var context =
            CreateContext();

        var service = CreateService(context);

        await Assert.ThrowsAsync<
            ArgumentException>(
                () =>
                    service
                        .ReviewCompanyVerificationAsync(
                            "admin-1",
                            Guid.NewGuid(),
                            "Suspended"));
    }
}
