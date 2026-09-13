using DevSphere.Application.DTOs.Profile;
using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Entities.Resume;
using DevSphere.Domain.Entities.Skills;
using DevSphere.Domain.Entities.Taxonomy;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Matching;
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
        var companyId = Guid.NewGuid();
        var employerProfileId = Guid.NewGuid();

        context.Users.Add(new ApplicationUser
        {
            Id = "employer-1",
            UserName = "employer@example.com",
            NormalizedUserName = "EMPLOYER@EXAMPLE.COM",
            Email = "employer@example.com",
            NormalizedEmail = "EMPLOYER@EXAMPLE.COM",
            IsActive = true,
            EmailConfirmed = true
        });
        context.EmployerProfiles.Add(new EmployerProfile
        {
            Id = employerProfileId,
            UserId = "employer-1",
            CompanyName = "Verified Company"
        });
        context.CompanyProfiles.Add(new CompanyProfile
        {
            Id = companyId,
            Name = "Verified Company"
        });
        context.CompanyMemberships.Add(new CompanyMembership
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            EmployerUserId = "employer-1",
            Status = CompanyMembershipStatus.Verified
        });
        context.CompanyVerifications.Add(new CompanyVerification
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            EmployerProfileId = employerProfileId,
            Status = CompanyVerificationStatus.Verified.ToString(),
            SubmittedAtUtc = DateTime.UtcNow
        });
        context.SaveChanges();

        var vacancyRepository =
            new VacancyRepository(context);

        var policyService =
            new VacancyPolicyService(
                new VacancyPolicyRepository(context),
                context);

        return new VacancyService(
            vacancyRepository,
            policyService,
            context);
    }

    private static VacancyDto CreateRequest()
    {
        return new VacancyDto
        {
            Title = "Backend Engineer",
            Description = "Build APIs",
            Location = "Colombo",
            WorkMode = "Hybrid",
            EmploymentType = "Full-time",
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

        await AddUsablePolicyAsync(context, created.Id);

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
    public async Task PublishedVacancy_Produces_Calculated_Match_With_User_Facing_Reason()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var conceptId = Guid.NewGuid();
        var resumeId = Guid.NewGuid();
        var resumeVersionId = Guid.NewGuid();
        context.Users.Add(new ApplicationUser
        {
            Id = "candidate-1",
            UserName = "candidate@example.com",
            NormalizedUserName = "CANDIDATE@EXAMPLE.COM",
            Email = "candidate@example.com",
            NormalizedEmail = "CANDIDATE@EXAMPLE.COM",
            IsActive = true,
            EmailConfirmed = true
        });
        context.SkillConcepts.Add(new SkillConcept
        {
            Id = conceptId,
            Name = "C#",
            NormalizedName = "C#",
            IsActive = true
        });
        var candidate = new CandidateProfile
        {
            Id = Guid.NewGuid(),
            UserId = "candidate-1",
            FullName = "Candidate One",
            Education = "Degree",
            PreferredWorkMode = "Hybrid",
            PreferredLocation = "Colombo",
            PreferredEmploymentType = "Full-time",
            AvailabilityStatus = "Immediately",
            Skills = new List<Skill>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "C#",
                    SkillConceptId = conceptId
                }
            }
        };
        context.CandidateProfiles.Add(candidate);
        context.Resumes.Add(new Resume
        {
            Id = resumeId,
            CandidateProfileId = candidate.Id,
            CurrentVersionId = resumeVersionId,
            Versions = new List<ResumeVersion>
            {
                new()
                {
                    Id = resumeVersionId,
                    ResumeId = resumeId,
                    VersionNumber = 1,
                    OriginalFileName = "candidate.pdf",
                    StorageKey = "candidate-v1.pdf",
                    ContentType = "application/pdf",
                    FileSizeBytes = 1024,
                    IsCurrent = true
                }
            }
        });
        await context.SaveChangesAsync();

        var created = await service.CreateAsync("employer-1", CreateRequest());
        await AddUsablePolicyAsync(context, created.Id);
        await service.PublishAsync("employer-1", created.Id);

        var engine = new MatchEngineService(new MatchingRepository(context));
        var result = await engine.CalculateAsync("candidate-1", created.Id.ToString());

        Assert.Equal(
            DevSphere.Application.DTOs.Application.MatchAssessmentStatus.Calculated,
            result.AssessmentStatus);
        Assert.Equal(
            DevSphere.Application.DTOs.Application.MatchEligibilityStatus.MeetsBaseline,
            result.Eligibility);
        Assert.Equal("AllMandatoryRequirementsSatisfied", result.EligibilityReason);
        Assert.Equal(100m, result.DisplayCompatibility);

        var candidateRepository = new CandidateProfileRepository(context);
        var resumeRepository = new ResumeRepository(context);
        var applicationService = new JobApplicationService(
            new JobApplicationRepository(context),
            null!,
            engine,
            null!,
            new CandidateProfileService(
                candidateRepository,
                new SkillTaxonomyService(context),
                resumeRepository),
            candidateRepository,
            resumeRepository,
            new VacancyRepository(context),
            context);
        var applyDecision = await applicationService.GetApplyDecisionAsync(
            "candidate-1",
            created.Id);

        Assert.True(applyDecision.CanSubmit);
        Assert.Equal("Allowed", applyDecision.PrimaryCode);
        Assert.Equal(resumeVersionId, applyDecision.ResumeVersionId);
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
    public async Task IncompleteDraft_CanBeSaved_ButCannotBePublished()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var request = CreateRequest();
        request.Description = string.Empty;
        request.WorkMode = string.Empty;
        request.ClosingDateUtc = null;

        var created = await service.CreateAsync("employer-1", request);

        Assert.Equal("Draft", created.LifecycleStatus);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PublishAsync("employer-1", created.Id));
        Assert.Contains("description", error.Message);
        Assert.Contains("work mode", error.Message);
        Assert.Contains("closing date", error.Message);
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

        await AddUsablePolicyAsync(context, created.Id);

        await service.PublishAsync(
            "employer-1",
            created.Id);

        var closed =
            await service.CloseAsync(
                "employer-1",
                created.Id);

        Assert.Equal("Closed", closed.LifecycleStatus);
        Assert.False(closed.IsOpen);
        Assert.Equal("Verified Company", closed.CompanyName);
        Assert.Equal("Verified", closed.CompanyVerificationStatus);

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

        await AddUsablePolicyAsync(context, created.Id);

        await service.PublishAsync(
            "employer-1",
            created.Id);

        var policyService =
            new VacancyPolicyService(
                new VacancyPolicyRepository(context),
                context);

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

        await AddUsablePolicyAsync(context, created.Id);

        await service.PublishAsync(
            "employer-1",
            created.Id);

        var policyService =
            new VacancyPolicyService(
                new VacancyPolicyRepository(context),
                context);

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

        await AddUsablePolicyAsync(context, created.Id);

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

    [Fact]
    public async Task Draft_With_Title_Only_Can_Be_Saved()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var request = new VacancyDto
        {
            Title = "Early draft",
            RequiredSkills = new List<VacancyRequiredSkillDto>()
        };

        var created = await service.CreateAsync("employer-1", request);

        Assert.Equal("Draft", created.LifecycleStatus);
        Assert.Empty(created.RequiredSkills);
    }

    [Fact]
    public async Task Publish_With_Zero_Policy_Families_And_No_Skills_Is_Rejected()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var request = CreateRequest();
        request.RequiredSkills.Clear();
        var created = await service.CreateAsync("employer-1", request);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PublishAsync("employer-1", created.Id));

        Assert.Contains("required skill", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Draft", (await context.Vacancies.FindAsync(created.Id))!.LifecycleStatus.ToString());
    }

    [Fact]
    public async Task Publish_With_Required_Skill_Only_Is_Rejected()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var created = await service.CreateAsync("employer-1", CreateRequest());

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PublishAsync("employer-1", created.Id));

        Assert.Contains("active, scored matching-policy family", error.Message);
        Assert.Equal("Draft", (await context.Vacancies.FindAsync(created.Id))!.LifecycleStatus.ToString());
    }

    [Fact]
    public async Task Publish_With_Inactive_Policy_Family_Is_Rejected()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var created = await service.CreateAsync("employer-1", CreateRequest());
        await AddUsablePolicyAsync(context, created.Id, familyActive: false);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PublishAsync("employer-1", created.Id));

        Assert.Contains("active, scored", error.Message);
    }

    [Fact]
    public async Task Publish_With_NonScored_Policy_Family_Is_Rejected()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var created = await service.CreateAsync("employer-1", CreateRequest());
        await AddUsablePolicyAsync(context, created.Id, familyScored: false);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PublishAsync("employer-1", created.Id));

        Assert.Contains("active, scored", error.Message);
    }

    [Fact]
    public async Task Publish_With_Valid_Scored_Family_Produces_Usable_Published_State()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var created = await service.CreateAsync("employer-1", CreateRequest());
        await AddUsablePolicyAsync(context, created.Id);

        var published = await service.PublishAsync("employer-1", created.Id);

        Assert.Equal("Published", published.LifecycleStatus);
        Assert.True(published.IsOpen);
        Assert.True(await context.VacancyRequirements.AnyAsync(x =>
            x.VacancyId == published.Id && x.IsActive && x.IsScored));
    }

    private static async Task AddUsablePolicyAsync(
        DevSphereDbContext context,
        Guid vacancyId,
        bool familyActive = true,
        bool familyScored = true)
    {
        var revision = new MatchingPolicyRevision
        {
            Id = Guid.NewGuid(),
            VacancyId = vacancyId,
            RevisionNumber = 1,
            IsCurrent = true
        };
        var family = new FamilyPolicy
        {
            Id = Guid.NewGuid(),
            MatchingPolicyRevisionId = revision.Id,
            RequirementFamily = RequirementFamily.Skill,
            FamilyImportance = RequirementImportance.High,
            IsActive = familyActive,
            IsScored = familyScored
        };
        context.MatchingPolicyRevisions.Add(revision);
        context.FamilyPolicies.Add(family);
        context.VacancyRequirements.Add(new VacancyRequirement
        {
            Id = Guid.NewGuid(),
            VacancyId = vacancyId,
            MatchingPolicyRevisionId = revision.Id,
            FamilyPolicyId = family.Id,
            RequirementFamily = RequirementFamily.Skill,
            Mode = RequirementMode.Mandatory,
            Importance = RequirementImportance.High,
            IsActive = true,
            IsScored = true,
            Description = "C#",
            CanonicalTargetKey = "c#"
        });
        await context.SaveChangesAsync();
    }
}
