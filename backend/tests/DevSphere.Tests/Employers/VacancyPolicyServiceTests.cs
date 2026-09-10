using DevSphere.Application.DTOs.Employers;
using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Services.Employers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevSphere.Tests.Employers;

public class VacancyPolicyServiceTests
{
    private static DevSphereDbContext CreateContext()
    {
        var options =
            new DbContextOptionsBuilder<DevSphereDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new DevSphereDbContext(options);
    }

    private static VacancyPolicyService CreateService(
        DevSphereDbContext context)
    {
        return new VacancyPolicyService(
            new VacancyPolicyRepository(context),
            context);
    }

    private static async Task<Vacancy> SeedVacancyAsync(
        DevSphereDbContext context,
        string employerId = "employer-1",
        VacancyLifecycleStatus status =
            VacancyLifecycleStatus.Draft)
    {
        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            EmployerId = employerId,
            Title = "Backend Engineer",
            Description = "API development",
            Location = "Colombo",
            MinExperienceMonths = 12,
            RequiredExperienceMonths = 12,
            RequiredEducation = "Degree",
            LifecycleStatus = status,
            IsOpen =
                status == VacancyLifecycleStatus.Published,
            CreatedAt = DateTime.UtcNow
        };

        context.Vacancies.Add(vacancy);
        await context.SaveChangesAsync();

        return vacancy;
    }

    [Fact]
    public async Task GetCurrentRevision_CreatesRevisionOne()
    {
        await using var context = CreateContext();
        var vacancy = await SeedVacancyAsync(context);
        var service = CreateService(context);

        var revision =
            await service.GetCurrentRevisionAsync(
                "employer-1",
                vacancy.Id);

        Assert.Equal(1, revision.RevisionNumber);
        Assert.True(revision.IsCurrent);
        Assert.False(revision.IsMateriallyLocked);
    }

    [Fact]
    public async Task PublishedMaterialPolicyEdit_CreatesSingleSuccessorRevision()
    {
        await using var context = CreateContext();

        var vacancy = await SeedVacancyAsync(
            context,
            status: VacancyLifecycleStatus.Published);

        var service = CreateService(context);

        var revision1 =
            await service.GetCurrentRevisionAsync(
                "employer-1",
                vacancy.Id);

        var family =
            await service.AddFamilyPolicyAsync(
                "employer-1",
                vacancy.Id,
                new FamilyPolicyRequest
                {
                    RequirementFamily =
                        RequirementFamily.Skill,
                    FamilyImportance =
                        RequirementImportance.High
                });

        Assert.NotEqual(
            revision1.Id,
            family.MatchingPolicyRevisionId);

        var current =
            await service.GetCurrentRevisionAsync(
                "employer-1",
                vacancy.Id);

        Assert.Equal(2, current.RevisionNumber);

        await service.AddFamilyPolicyAsync(
            "employer-1",
            vacancy.Id,
            new FamilyPolicyRequest
            {
                RequirementFamily =
                    RequirementFamily.Experience,
                FamilyImportance =
                    RequirementImportance.Medium
            });

        Assert.Equal(
            2,
            await context.MatchingPolicyRevisions.CountAsync());
    }

    [Fact]
    public async Task DuplicateFamilyPolicy_IsRejected()
    {
        await using var context = CreateContext();
        var vacancy = await SeedVacancyAsync(context);
        var service = CreateService(context);

        var request = new FamilyPolicyRequest
        {
            RequirementFamily =
                RequirementFamily.Skill,
            FamilyImportance =
                RequirementImportance.High
        };

        await service.AddFamilyPolicyAsync(
            "employer-1",
            vacancy.Id,
            request);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddFamilyPolicyAsync(
                "employer-1",
                vacancy.Id,
                request));
    }

    [Fact]
    public async Task MinSatisfied_WithInvalidK_IsRejected()
    {
        await using var context = CreateContext();
        var vacancy = await SeedVacancyAsync(context);
        var service = CreateService(context);

        var family =
            await service.AddFamilyPolicyAsync(
                "employer-1",
                vacancy.Id,
                new FamilyPolicyRequest
                {
                    RequirementFamily =
                        RequirementFamily.Skill,
                    FamilyImportance =
                        RequirementImportance.High
                });

        var requirement =
            await service.AddRequirementAsync(
                "employer-1",
                vacancy.Id,
                new VacancyRequirementRequest
                {
                    FamilyPolicyId = family.Id,
                    RequirementFamily =
                        RequirementFamily.Skill,
                    Mode = RequirementMode.Preferred,
                    Importance =
                        RequirementImportance.High,
                    CanonicalTargetKey = "csharp",
                    Description = "C#",
                    IsScored = true
                });

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddAlternativeSetAsync(
                "employer-1",
                vacancy.Id,
                new AlternativeSetRequest
                {
                    FamilyPolicyId = family.Id,
                    SetType =
                        AlternativeSetType.MinSatisfied,
                    MinimumSatisfiedCount = 2,
                    Mode = RequirementMode.Preferred,
                    Importance =
                        RequirementImportance.High,
                    IsScored = true,
                    MemberRequirementIds =
                        new List<Guid>
                        {
                            requirement.Id
                        }
                }));
    }

    [Fact]
    public async Task PolicyMutation_AfterFirstApplication_IsLocked()
    {
        await using var context = CreateContext();

        var vacancy = await SeedVacancyAsync(
            context,
            status: VacancyLifecycleStatus.Published);

        var service = CreateService(context);

        await service.GetCurrentRevisionAsync(
            "employer-1",
            vacancy.Id);

        context.JobApplications.Add(
            new JobApplication
            {
                Id = Guid.NewGuid(),
                CandidateId = "candidate-1",
                VacancyId = vacancy.Id,
                Status = ApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddFamilyPolicyAsync(
                "employer-1",
                vacancy.Id,
                new FamilyPolicyRequest
                {
                    RequirementFamily =
                        RequirementFamily.Skill,
                    FamilyImportance =
                        RequirementImportance.High
                }));

        var revision =
            await context.MatchingPolicyRevisions
                .SingleAsync(x => x.IsCurrent);

        Assert.True(revision.IsMateriallyLocked);
        Assert.NotNull(revision.MateriallyLockedAtUtc);
    }
}
