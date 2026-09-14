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

public class EmployerWorkflowServiceTests
{
    private static DevSphereDbContext CreateContext()
    {
        var options =
            new DbContextOptionsBuilder<DevSphereDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new DevSphereDbContext(options);
    }

    private static EmployerWorkflowService CreateService(
        DevSphereDbContext context)
    {
        return new EmployerWorkflowService(
            new EmployerWorkflowRepository(context));
    }

    private static async Task<JobApplication>
        SeedApplicationAsync(
            DevSphereDbContext context,
            string employerId = "employer-1")
    {
        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            EmployerId = employerId,
            Title = "Backend Engineer",
            Description = "API development",
            Location = "Colombo",
            RequiredEducation = "Degree",
            MinExperienceMonths = 12,
            RequiredExperienceMonths = 12,
            LifecycleStatus =
                VacancyLifecycleStatus.Published,
            IsOpen = true,
            CreatedAt = DateTime.UtcNow
        };

        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            CandidateId = "candidate-1",
            VacancyId = vacancy.Id,
            Vacancy = vacancy,
            Status = ApplicationStatus.Applied,
            AppliedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        context.Vacancies.Add(vacancy);
        context.JobApplications.Add(application);

        await context.SaveChangesAsync();

        return application;
    }

    [Fact]
    public async Task ForeignEmployer_CannotCreateInterview()
    {
        await using var context = CreateContext();
        var application =
            await SeedApplicationAsync(context);

        var service = CreateService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateInterviewAsync(
                "employer-2",
                new CreateInterviewRequest
                {
                    JobApplicationId = application.Id
                }));
    }

    [Fact]
    public async Task Interview_UsesScheduledCompletedTerminalFlow()
    {
        await using var context = CreateContext();
        var application =
            await SeedApplicationAsync(context);

        var service = CreateService(context);

        var interview =
            await service.CreateInterviewAsync(
                "employer-1",
                new CreateInterviewRequest
                {
                    JobApplicationId = application.Id
                });

        Assert.Equal("Scheduled", interview.Status);

        var completed =
            await service.UpdateInterviewStatusAsync(
                "employer-1",
                interview.Id,
                new UpdateInterviewStatusRequest
                {
                    Status = "Completed"
                });

        Assert.Equal("Completed", completed.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateInterviewStatusAsync(
                "employer-1",
                interview.Id,
                new UpdateInterviewStatusRequest
                {
                    Status = "Cancelled"
                }));
    }

    [Fact]
    public async Task Scorecard_WithScheduledInterview_IsRejected()
    {
        await using var context = CreateContext();
        var application =
            await SeedApplicationAsync(context);

        var service = CreateService(context);

        var interview =
            await service.CreateInterviewAsync(
                "employer-1",
                new CreateInterviewRequest
                {
                    JobApplicationId = application.Id
                });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateScorecardAsync(
                "employer-1",
                new CreateScorecardRequest
                {
                    JobApplicationId = application.Id,
                    InterviewId = interview.Id,
                    OverallRating = 4
                }));
    }

    [Fact]
    public async Task CompletedInterview_AllowsHumanScorecard()
    {
        await using var context = CreateContext();
        var application =
            await SeedApplicationAsync(context);

        var service = CreateService(context);

        var interview =
            await service.CreateInterviewAsync(
                "employer-1",
                new CreateInterviewRequest
                {
                    JobApplicationId = application.Id
                });

        await service.UpdateInterviewStatusAsync(
            "employer-1",
            interview.Id,
            new UpdateInterviewStatusRequest
            {
                Status = "Completed"
            });

        var scorecard =
            await service.CreateScorecardAsync(
                "employer-1",
                new CreateScorecardRequest
                {
                    JobApplicationId = application.Id,
                    InterviewId = interview.Id,
                    OverallRating = 5,
                    Notes = "Strong technical interview"
                });

        Assert.Equal(5, scorecard.OverallRating);

        var unchangedApplication =
            await context.JobApplications
                .SingleAsync(x => x.Id == application.Id);

        Assert.Equal(
            ApplicationStatus.Applied,
            unchangedApplication.Status);
    }

    [Fact]
    public async Task Offer_UsesExplicitStateMachineWithoutChangingApplicationStatus()
    {
        await using var context = CreateContext();
        var application =
            await SeedApplicationAsync(context);

        var service = CreateService(context);

        var offer =
            await service.CreateOfferAsync(
                "employer-1",
                new CreateOfferRequest
                {
                    JobApplicationId = application.Id,
                    OfferedSalary = 250000,
                    ExpiresAtUtc =
                        DateTime.UtcNow.AddDays(7)
                });

        Assert.Equal("Draft", offer.Status);

        offer = await service.UpdateOfferStatusAsync(
            "employer-1",
            offer.Id,
            new UpdateOfferStatusRequest
            {
                Status = "Extended"
            });

        Assert.Equal("Extended", offer.Status);

        offer = await service.UpdateOfferStatusAsync(
            "employer-1",
            offer.Id,
            new UpdateOfferStatusRequest
            {
                Status = "Accepted"
            });

        Assert.Equal("Accepted", offer.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateOfferStatusAsync(
                "employer-1",
                offer.Id,
                new UpdateOfferStatusRequest
                {
                    Status = "Declined"
                }));

        var unchangedApplication =
            await context.JobApplications
                .SingleAsync(x => x.Id == application.Id);

        Assert.Equal(
            ApplicationStatus.Applied,
            unchangedApplication.Status);
    }

    [Fact]
    public async Task TalentPool_RequiresConsentAndDerivesCandidate()
    {
        await using var context = CreateContext();
        var application =
            await SeedApplicationAsync(context);

        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddTalentPoolEntryAsync(
                "employer-1",
                new CreateTalentPoolEntryRequest
                {
                    JobApplicationId = application.Id,
                    HasCandidateConsent = false
                }));

        var entry =
            await service.AddTalentPoolEntryAsync(
                "employer-1",
                new CreateTalentPoolEntryRequest
                {
                    JobApplicationId = application.Id,
                    HasCandidateConsent = true
                });

        Assert.Equal(
            application.CandidateId,
            entry.CandidateUserId);

        Assert.True(entry.HasCandidateConsent);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddTalentPoolEntryAsync(
                "employer-1",
                new CreateTalentPoolEntryRequest
                {
                    JobApplicationId = application.Id,
                    HasCandidateConsent = true
                }));
    }
}
