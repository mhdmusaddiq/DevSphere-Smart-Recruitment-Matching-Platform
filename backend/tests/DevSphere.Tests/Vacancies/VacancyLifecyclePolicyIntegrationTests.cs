using DevSphere.Application.DTOs.Profile;
using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Services.Employers;
using DevSphere.Infrastructure.Services.Profile;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevSphere.Tests.Vacancies;

public class VacancyLifecyclePolicyIntegrationTests
{
    private static DevSphereDbContext CreateContext()
    {
        var options =
            new DbContextOptionsBuilder<DevSphereDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new DevSphereDbContext(options);
    }

    private static VacancyService CreateService(
        DevSphereDbContext context)
    {
        var vacancyRepository =
            new VacancyRepository(context);

        var policyService =
            new VacancyPolicyService(
                new VacancyPolicyRepository(context));

        return new VacancyService(
            vacancyRepository,
            policyService);
    }

    private static VacancyDto CreateRequest()
    {
        return new VacancyDto
        {
            Title = "Backend Engineer",
            Description = "Build APIs",
            Location = "Colombo",
            MinExperienceMonths = 12,
            MaxExperienceMonths = 36,
            RequiredEducation = "Degree",
            SalaryMin = 150000,
            SalaryMax = 300000,
            ClosingDateUtc =
                DateTime.UtcNow.AddDays(30),
            RequiredSkills =
                new List<VacancyRequiredSkillDto>
                {
                    new VacancyRequiredSkillDto
                    {
                        Name = "C#",
                        Weight = 5
                    }
                }
        };
    }

    [Fact]
    public async Task CreateThenPublish_UsesDraftToPublishedLifecycle()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var created =
            await service.CreateAsync(
                "employer-1",
                CreateRequest());

        Assert.Equal("Draft", created.LifecycleStatus);
        Assert.False(created.IsOpen);

        var published =
            await service.PublishAsync(
                "employer-1",
                created.Id);

        Assert.Equal(
            "Published",
            published.LifecycleStatus);

        Assert.True(published.IsOpen);
    }

    [Fact]
    public async Task DraftVacancy_CannotBeClosed()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var created =
            await service.CreateAsync(
                "employer-1",
                CreateRequest());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CloseAsync(
                "employer-1",
                created.Id));
    }

    [Fact]
    public async Task PublishedVacancy_CloseIsTerminal()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var created =
            await service.CreateAsync(
                "employer-1",
                CreateRequest());

        await service.PublishAsync(
            "employer-1",
            created.Id);

        var closed =
            await service.CloseAsync(
                "employer-1",
                created.Id);

        Assert.Equal("Closed", closed.LifecycleStatus);
        Assert.False(closed.IsOpen);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PublishAsync(
                "employer-1",
                created.Id));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(
                "employer-1",
                created.Id,
                CreateRequest()));
    }

    [Fact]
    public async Task PublishedMaterialEditBeforeApplication_CreatesRevisionTwo()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var created =
            await service.CreateAsync(
                "employer-1",
                CreateRequest());

        await service.PublishAsync(
            "employer-1",
            created.Id);

        var policyService =
            new VacancyPolicyService(
                new VacancyPolicyRepository(context));

        var revision1 =
            await policyService.GetCurrentRevisionAsync(
                "employer-1",
                created.Id);

        Assert.Equal(1, revision1.RevisionNumber);

        var update = CreateRequest();
        update.Location = "Kandy";

        var updated =
            await service.UpdateAsync(
                "employer-1",
                created.Id,
                update);

        Assert.Equal("Kandy", updated.Location);

        var current =
            await policyService.GetCurrentRevisionAsync(
                "employer-1",
                created.Id);

        Assert.Equal(2, current.RevisionNumber);

        Assert.Equal(
            2,
            await context.MatchingPolicyRevisions
                .CountAsync());
    }

    [Fact]
    public async Task MaterialEditAfterApplication_IsRejected()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var created =
            await service.CreateAsync(
                "employer-1",
                CreateRequest());

        await service.PublishAsync(
            "employer-1",
            created.Id);

        var policyService =
            new VacancyPolicyService(
                new VacancyPolicyRepository(context));

        await policyService.GetCurrentRevisionAsync(
            "employer-1",
            created.Id);

        context.JobApplications.Add(
            new JobApplication
            {
                Id = Guid.NewGuid(),
                CandidateId = "candidate-1",
                VacancyId = created.Id,
                Status = ApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        var update = CreateRequest();
        update.Location = "Kandy";

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(
                "employer-1",
                created.Id,
                update));
    }

    [Fact]
    public async Task NonMaterialEditAfterApplication_RemainsAllowed()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var created =
            await service.CreateAsync(
                "employer-1",
                CreateRequest());

        await service.PublishAsync(
            "employer-1",
            created.Id);

        context.JobApplications.Add(
            new JobApplication
            {
                Id = Guid.NewGuid(),
                CandidateId = "candidate-1",
                VacancyId = created.Id,
                Status = ApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        var update = CreateRequest();

        // Title/description are deliberately non-material.
        update.Title = "Senior Backend Engineer";
        update.Description =
            "Updated non-material description";

        var result =
            await service.UpdateAsync(
                "employer-1",
                created.Id,
                update);

        Assert.Equal(
            "Senior Backend Engineer",
            result.Title);

        Assert.Equal(
            "Updated non-material description",
            result.Description);
    }
}
