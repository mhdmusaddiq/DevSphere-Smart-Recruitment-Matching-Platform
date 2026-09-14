using DevSphere.Application.DTOs.Employers;
using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Entities.Contacts;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Repositories.Contacts;
using DevSphere.Infrastructure.Services.Auth;
using DevSphere.Infrastructure.Services.Contacts;
using DevSphere.Infrastructure.Services.Employers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevSphere.Tests.Security;

public class FrontendEnablementSurgicalTests
{
    private static DevSphereDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DevSphereDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DevSphereDbContext(options);
    }

    private static string ReadRepoFile(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            var candidate = Path.Combine(
                new[] { directory.FullName, "backend" }.Concat(parts).ToArray());

            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException();
    }

    [Fact]
    public void Apply_Uses_Dedicated_Request_Shared_Preflight_And_Acknowledgement()
    {
        var controller = ReadRepoFile(
            "src", "DevSphere.Api", "Controllers", "Profile",
            "JobApplicationController.cs");
        var service = ReadRepoFile(
            "src", "DevSphere.Infrastructure", "Services", "Profile",
            "JobApplicationService.cs");
        var dto = ReadRepoFile(
            "src", "DevSphere.Application", "DTOs", "Application",
            "JobApplicationDto.cs");

        Assert.Contains(
            "[HttpGet(\"vacancies/{vacancyId:guid}/apply-decision\")]",
            controller);
        Assert.Contains("ApplyApplicationRequest request", controller);
        Assert.Contains("public class ApplyApplicationRequest", dto);
        Assert.Contains("BaselineAcknowledged", dto);
        Assert.True(
            service.Split("EvaluateApplyDecisionCoreAsync(").Length >= 3);
        Assert.Contains("!request.BaselineAcknowledged", service);
        Assert.Contains("AddReasonIf", service);
    }

    [Fact]
    public void Snapshot_And_History_Are_Read_Only_Public_Endpoints()
    {
        var source = ReadRepoFile(
            "src", "DevSphere.Api", "Controllers", "Applications",
            "ApplicationHistoryController.cs");

        Assert.Contains("[HttpGet(\"snapshot\")]", source);
        Assert.Contains("[HttpGet(\"status-history\")]", source);
        Assert.DoesNotContain("[HttpPost(\"snapshot\")]", source);
        Assert.DoesNotContain("[HttpPost(\"status-history\")]", source);
    }

    [Fact]
    public async Task Candidate_Can_Revoke_Accepted_Consent_And_Employer_Cannot()
    {
        await using var context = CreateContext();
        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            EmployerId = "employer-1",
            Title = "Engineer"
        };
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            CandidateId = "candidate-1",
            VacancyId = vacancy.Id,
            Vacancy = vacancy,
            Status = ApplicationStatus.Submitted
        };
        var accepted = new ContactRequest
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.Id,
            CandidateId = "candidate-1",
            EmployerId = "employer-1",
            Status = "Accepted"
        };
        context.AddRange(vacancy, application, accepted);
        await context.SaveChangesAsync();

        var service = new ContactRequestService(
            new ContactRequestRepository(context),
            new JobApplicationRepository(context),
            context);

        var revoked = await service.UpdateStatusAsync(
            accepted.Id,
            "Revoked",
            "candidate-1");
        Assert.Equal("Revoked", revoked.Status);

        accepted.Status = "Accepted";
        await context.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateEmployerStatusAsync(
                accepted.Id,
                "Revoked",
                "employer-1"));
    }

    [Fact]
    public async Task Employer_Can_Only_Cancel_Pending_Contact_Request()
    {
        await using var context = CreateContext();
        var request = new ContactRequest
        {
            Id = Guid.NewGuid(),
            JobApplicationId = Guid.NewGuid(),
            CandidateId = "candidate-1",
            EmployerId = "employer-1",
            Status = "Pending"
        };
        context.ContactRequests.Add(request);
        await context.SaveChangesAsync();
        var service = new ContactRequestService(
            new ContactRequestRepository(context),
            new JobApplicationRepository(context),
            context);

        var result = await service.UpdateEmployerStatusAsync(
            request.Id,
            "Cancelled",
            "employer-1");

        Assert.Equal("Cancelled", result.Status);
    }

    [Fact]
    public async Task Company_Edit_Preserves_Server_Owned_Trust_State()
    {
        await using var context = CreateContext();
        var company = new CompanyProfile
        {
            Id = Guid.NewGuid(),
            Name = "Old"
        };
        var membership = new CompanyMembership
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            EmployerUserId = "employer-1",
            Status = CompanyMembershipStatus.Verified
        };
        var verification = new CompanyVerification
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Status = CompanyVerificationStatus.Verified.ToString(),
            EvidenceStorageKey = "evidence.pdf",
            SubmittedAtUtc = DateTime.UtcNow
        };
        context.AddRange(company, membership, verification);
        await context.SaveChangesAsync();

        var service = new CompanyTrustService(
            new CompanyTrustRepository(context));
        var result = await service.UpdateCompanyAsync(
            "employer-1",
            company.Id,
            new UpdateCompanyProfileRequest
            {
                Name = "Updated",
                Description = "Facts",
                Website = "https://example.test",
                Location = "Colombo"
            });

        Assert.Equal("Updated", result.Name);
        Assert.Equal("Verified", result.MembershipStatus);
        Assert.Equal("Verified", result.VerificationStatus);
    }

    [Fact]
    public async Task Policy_Aggregate_Replaces_Graph_And_Locks_After_Application()
    {
        await using var context = CreateContext();
        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            EmployerId = "employer-1",
            LifecycleStatus = VacancyLifecycleStatus.Draft,
            Title = "Engineer"
        };
        context.Vacancies.Add(vacancy);
        await context.SaveChangesAsync();
        var service = new VacancyPolicyService(
            new VacancyPolicyRepository(context),
            context);
        var request = new VacancyPolicyAggregateUpdateRequest
        {
            Families =
            [
                new FamilyPolicyEditRequest
                {
                    ClientKey = "skills",
                    RequirementFamily = RequirementFamily.Skill,
                    FamilyImportance = RequirementImportance.High
                }
            ],
            Requirements =
            [
                new VacancyRequirementEditRequest
                {
                    ClientKey = "dotnet",
                    FamilyClientKey = "skills",
                    RequirementFamily = RequirementFamily.Skill,
                    Mode = RequirementMode.Mandatory,
                    Importance = RequirementImportance.High,
                    Description = ".NET",
                    CanonicalTargetKey = "dotnet"
                }
            ]
        };

        var aggregate = await service.ReplaceCurrentAggregateAsync(
            "employer-1",
            vacancy.Id,
            request);
        Assert.Single(aggregate.Families);
        Assert.Single(aggregate.Requirements);
        Assert.Empty(aggregate.AlternativeSets);

        context.JobApplications.Add(new JobApplication
        {
            Id = Guid.NewGuid(),
            CandidateId = "candidate-1",
            VacancyId = vacancy.Id,
            Status = ApplicationStatus.Submitted
        });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ReplaceCurrentAggregateAsync(
                "employer-1",
                vacancy.Id,
                request));
    }

    [Fact]
    public async Task Admin_User_Filter_Counts_The_Filtered_Set_Before_Paging()
    {
        await using var context = CreateContext();
        var employerRole = new ApplicationRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = AppRoles.Employer,
            NormalizedName = AppRoles.Employer.ToUpperInvariant()
        };
        var activeEmployer = new ApplicationUser
        {
            Id = "active-employer",
            Email = "match@example.test",
            UserName = "match@example.test",
            DisplayName = "Match Employer",
            IsActive = true
        };
        var inactiveEmployer = new ApplicationUser
        {
            Id = "inactive-employer",
            Email = "other@example.test",
            UserName = "other@example.test",
            DisplayName = "Other Employer",
            IsActive = false
        };
        context.AddRange(employerRole, activeEmployer, inactiveEmployer);
        context.UserRoles.AddRange(
            new IdentityUserRole<string>
            {
                UserId = activeEmployer.Id,
                RoleId = employerRole.Id
            },
            new IdentityUserRole<string>
            {
                UserId = inactiveEmployer.Id,
                RoleId = employerRole.Id
            });
        await context.SaveChangesAsync();

        var page = await new AdminRepository(context).GetUsersAsync(
            "match",
            AppRoles.Employer,
            true,
            1,
            1);

        Assert.Equal(1, page.TotalCount);
        Assert.Single(page.Users);
        Assert.Equal(activeEmployer.Id, page.Users[0].Id);
    }

    [Fact]
    public void Private_Evidence_Has_Admin_Id_Route_And_No_Generic_Download()
    {
        var admin = ReadRepoFile(
            "src", "DevSphere.Api", "Controllers", "Administration",
            "AdminController.cs");
        var files = ReadRepoFile(
            "src", "DevSphere.Api", "Controllers", "Files",
            "FilesController.cs");

        Assert.Contains(
            "[HttpGet(\"company-verifications/{verificationId:guid}/evidence\")]",
            admin);
        Assert.DoesNotContain("[HttpGet(\"{storageKey}\")]", files);
    }

    [Fact]
    public void Employer_Detail_And_My_Applications_Expose_Frozen_Read_Fields()
    {
        var controller = ReadRepoFile(
            "src", "DevSphere.Api", "Controllers", "Profile",
            "JobApplicationController.cs");
        var dto = ReadRepoFile(
            "src", "DevSphere.Application", "DTOs", "Application",
            "JobApplicationDto.cs");

        Assert.Contains(
            "[HttpGet(\"/api/employer/applications/{applicationId:guid}\")]",
            controller);
        Assert.Contains("VacancyLocation", dto);
        Assert.Contains("WorkMode", dto);
        Assert.Contains("EmployerApplicationDetailDto", dto);
        Assert.DoesNotContain("CandidateEmail", dto);
    }

    [Fact]
    public void Smtp_Selection_And_Message_Composition_Are_Production_Safe()
    {
        var incomplete = new SmtpAuthChallengeOptions { Enabled = true };
        Assert.False(incomplete.IsComplete());

        var complete = new SmtpAuthChallengeOptions
        {
            Enabled = true,
            Host = "smtp.example.test",
            Port = 587,
            FromAddress = "no-reply@example.test"
        };
        Assert.True(complete.IsComplete());

        var message = SmtpAuthChallengeDelivery.Compose(
            "EmailVerification",
            "user@example.test",
            "123456");
        Assert.Contains("AptLens", message.Subject);
        Assert.Contains("123456", message.TextBody);
        Assert.Contains("123456", message.HtmlBody);
    }
}
