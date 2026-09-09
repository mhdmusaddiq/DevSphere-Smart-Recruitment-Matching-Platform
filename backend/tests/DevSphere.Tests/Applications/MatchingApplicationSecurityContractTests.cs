using DevSphere.Application.Exceptions;
using DevSphere.Infrastructure.Services.Profile;
using Xunit;

namespace DevSphere.Tests.Applications;

public class MatchingApplicationSecurityContractTests
{
    private static string ReadSource(
        params string[] parts)
    {
        var root = FindRepositoryRoot();

        var path = Path.Combine(
            new[] { root }
                .Concat(parts)
                .ToArray());

        Assert.True(
            File.Exists(path),
            $"Expected source file was not found: {path}");

        return File.ReadAllText(path);
    }


    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(
            AppContext.BaseDirectory);

        while (directory != null)
        {
            if (Directory.Exists(
                    Path.Combine(directory.FullName, "backend", "src")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Repository root could not be located.");
    }


    [Fact]
    public void DuplicateApplication_Should_Have_DedicatedConflictException()
    {
        Assert.True(
            typeof(ApplicationConflictException)
                .IsSubclassOf(typeof(Exception)));

        var source = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "JobApplicationService.cs");

        Assert.Contains(
            "ExistsAsync(",
            source);

        Assert.Contains(
            "throw new ApplicationConflictException",
            source);
    }


    [Fact]
    public void EmployerApplicantQuery_Should_Be_OwnershipScoped()
    {
        var source = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Repositories",
            "JobApplicationRepository.cs");

        Assert.Contains(
            "x.Vacancy.EmployerId == employerId",
            source);

        Assert.Contains(
            ".Include(x => x.Vacancy)",
            source);
    }


    [Fact]
    public void ContactRequest_Should_Derive_Candidate_From_Application()
    {
        var source = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Contacts",
            "ContactRequestService.cs");

        Assert.Contains(
            "application.Vacancy.EmployerId",
            source);

        Assert.Contains(
            "CandidateId = application.CandidateId",
            source);

        Assert.Contains(
            "ExistsForApplicationAsync",
            source);
    }


    [Fact]
    public void ForeignCandidate_Should_Not_Update_ContactRequest()
    {
        var repository = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Repositories",
            "Contacts",
            "ContactRequestRepository.cs");

        var service = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Contacts",
            "ContactRequestService.cs");

        Assert.Contains(
            "x.CandidateId == candidateId",
            repository);

        Assert.Contains(
            "GetForCandidateAsync(",
            service);
    }


    [Fact]
    public void RankedApplicants_Should_Use_Deterministic_Order()
    {
        var source = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "JobApplicationService.cs");

        Assert.Contains(
            ".OrderByDescending(x => x.RawCompatibility.HasValue)",
            source);

        Assert.Contains(
            ".ThenBy(x => x.AppliedAt)",
            source);

        Assert.Contains(
            ".ThenBy(x => x.ApplicationId)",
            source);
    }


    [Theory]
    [InlineData(
        DevSphere.Domain.Enums.ApplicationStatus.Applied,
        DevSphere.Domain.Enums.ApplicationStatus.UnderReview,
        true)]
    [InlineData(
        DevSphere.Domain.Enums.ApplicationStatus.UnderReview,
        DevSphere.Domain.Enums.ApplicationStatus.Shortlisted,
        true)]
    [InlineData(
        DevSphere.Domain.Enums.ApplicationStatus.Shortlisted,
        DevSphere.Domain.Enums.ApplicationStatus.Selected,
        true)]
    [InlineData(
        DevSphere.Domain.Enums.ApplicationStatus.Applied,
        DevSphere.Domain.Enums.ApplicationStatus.Selected,
        false)]
    [InlineData(
        DevSphere.Domain.Enums.ApplicationStatus.Selected,
        DevSphere.Domain.Enums.ApplicationStatus.Applied,
        false)]
    [InlineData(
        DevSphere.Domain.Enums.ApplicationStatus.Rejected,
        DevSphere.Domain.Enums.ApplicationStatus.UnderReview,
        false)]
    public void ApplicationWorkflow_Should_Enforce_TransitionRules(
        DevSphere.Domain.Enums.ApplicationStatus current,
        DevSphere.Domain.Enums.ApplicationStatus next,
        bool expected)
    {
        Assert.Equal(
            expected,
            JobApplicationService.IsValidTransition(
                current,
                next));
    }

    [Fact]
    public void EmployerCompare_Should_Be_Authorized_OwnershipScoped_And_ReadOnly()
    {
        var controller = ReadSource(
            "backend",
            "src",
            "DevSphere.Api",
            "Controllers",
            "Profile",
            "JobApplicationController.cs");

        var compareStart = controller.IndexOf(
            "public async Task<IActionResult> CompareCandidates",
            StringComparison.Ordinal);

        Assert.True(
            compareStart >= 0,
            "CompareCandidates action was not found.");

        var compareSection = controller.Substring(compareStart);

        Assert.Contains(
            "[HttpGet(\"vacancy/{vacancyId}/compare\")]",
            controller);

        Assert.Contains(
            "[Authorize(Roles = \"Employer\")]",
            controller);

        Assert.Contains(
            "ClaimTypes.NameIdentifier",
            compareSection);

        Assert.Contains(
            "GetByVacancyAsync(",
            compareSection);

        Assert.Contains(
            "vacancyId",
            compareSection);

        Assert.Contains(
            "employerId",
            compareSection);

        Assert.DoesNotContain(
            "ApplyAsync(",
            compareSection);

        Assert.DoesNotContain(
            "UpdateStatusAsync(",
            compareSection);

        Assert.DoesNotContain(
            "SaveChanges",
            compareSection);

        Assert.DoesNotContain(
            "ApplicationSnapshot",
            compareSection);
    }


    [Fact]
    public void EmployerCompare_Should_Use_Real_Applications_Only()
    {
        var repository = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Repositories",
            "JobApplicationRepository.cs");

        Assert.Contains(
            "_context.JobApplications",
            repository);

        Assert.Contains(
            "x.VacancyId == vacancyId",
            repository);

        Assert.Contains(
            "x.Vacancy.EmployerId == employerId",
            repository);

        Assert.DoesNotContain(
            "_context.CandidateProfiles",
            repository);
    }


    [Fact]
    public void EmployerCompare_Should_Expose_Canonical_Assessment_Evidence()
    {
        var dto = ReadSource(
            "backend",
            "src",
            "DevSphere.Application",
            "DTOs",
            "Application",
            "RankedApplicantDto.cs");

        var service = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "JobApplicationService.cs");

        Assert.Contains(
            "MatchAssessmentStatus AssessmentStatus",
            dto);

        Assert.Contains(
            "MatchEligibilityStatus Eligibility",
            dto);

        Assert.Contains(
            "EligibilityReason",
            dto);

        Assert.Contains(
            "decimal Coverage",
            dto);

        Assert.Contains(
            "MissingInputs",
            dto);

        Assert.Contains(
            "List<MatchFamilyResultDto> Families",
            dto);

        Assert.Contains(
            "RawCompatibility = match.RawCompatibility",
            service);

        Assert.Contains(
            "Eligibility = match.Eligibility",
            service);

        Assert.Contains(
            "EligibilityReason = match.EligibilityReason",
            service);

        Assert.Contains(
            "Coverage = match.Coverage",
            service);

        Assert.Contains(
            "Families = match.Families",
            service);
    }


    [Fact]
    public void EmployerCompare_Should_Preserve_Deterministic_Ranking_TieBreak()
    {
        var source = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "JobApplicationService.cs");

        var rawIndex = source.IndexOf(
            ".OrderByDescending(x => x.RawCompatibility.HasValue)",
            StringComparison.Ordinal);

        var appliedAtIndex = source.IndexOf(
            ".ThenBy(x => x.AppliedAt)",
            StringComparison.Ordinal);

        var applicationIdIndex = source.IndexOf(
            ".ThenBy(x => x.ApplicationId)",
            StringComparison.Ordinal);

        Assert.True(rawIndex >= 0);
        Assert.True(appliedAtIndex > rawIndex);
        Assert.True(applicationIdIndex > appliedAtIndex);
    }
}
