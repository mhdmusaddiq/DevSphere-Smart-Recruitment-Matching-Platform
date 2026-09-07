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
            ".OrderByDescending(x => x.MatchScore)",
            source);

        Assert.Contains(
            "StringComparer.Ordinal",
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
}
