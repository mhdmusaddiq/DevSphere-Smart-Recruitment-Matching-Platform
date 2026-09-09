namespace DevSphere.Tests.Security;

public class BackendClosureContractTests
{
    [Fact]
    public void Notification_Read_Should_Be_Recipient_Scoped()
    {
        var controller = Read("src", "DevSphere.Api", "Controllers",
            "Notifications", "NotificationController.cs");
        var repository = Read("src", "DevSphere.Infrastructure",
            "Repositories", "Notifications", "NotificationRepository.cs");

        Assert.Contains("ClaimTypes.NameIdentifier", controller);
        Assert.Contains("MarkAsReadAsync(\n            id,\n            userId)",
            controller.Replace("\r\n", "\n"));
        Assert.Contains("x.UserId == userId", repository);
    }

    [Fact]
    public void Application_Transition_Should_Be_Atomic_And_Canonical()
    {
        var source = Read("src", "DevSphere.Infrastructure", "Services",
            "Profile", "JobApplicationService.cs");

        Assert.Contains("IsolationLevel.Serializable", source);
        Assert.Contains("ApplicationStatusHistories.Add", source);
        Assert.Contains("Notifications.Add", source);
        Assert.Contains("contact.Status = \"Cancelled\"", source);
        Assert.Contains("ApplicationStatus.Screening", source);
        Assert.Contains("ApplicationStatus.Withdrawn", source);
    }

    [Fact]
    public void Contact_Disclosure_Should_Recheck_Live_Trust()
    {
        var source = Read("src", "DevSphere.Infrastructure", "Services",
            "Contacts", "ContactRequestService.cs");

        Assert.Contains("request.Status == \"Accepted\"", source);
        Assert.Contains("candidateUser?.IsActive == true", source);
        Assert.Contains("employerUser?.IsActive == true", source);
        Assert.Contains("CompanyMembershipStatus.Verified", source);
        Assert.Contains("CompanyVerificationStatus.Verified", source);
        Assert.Contains("\"Cancelled\"", source);
        Assert.Contains("\"Revoked\"", source);
    }

    [Fact]
    public void Real_View_Contracts_Should_Expose_Frozen_Context()
    {
        var application = Read("src", "DevSphere.Application", "DTOs",
            "Application", "JobApplicationDto.cs");
        var vacancy = Read("src", "DevSphere.Application", "DTOs",
            "Profile", "VacancyDto.cs");
        var workflow = Read("src", "DevSphere.Application", "DTOs",
            "Employers", "EmployerWorkflowDtos.cs");

        Assert.Contains("FrozenAssessmentStatus", application);
        Assert.Contains("ResumeVersionId", application);
        Assert.Contains("CompanyVerificationStatus", vacancy);
        Assert.Contains("PublishedAtUtc", vacancy);
        Assert.Contains("EmployerWorkflowSummaryDto", workflow);
    }

    private static string Read(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null &&
               !Directory.Exists(Path.Combine(directory.FullName, "src")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new DirectoryNotFoundException("Backend root not found.");
        }

        return File.ReadAllText(Path.Combine(
            new[] { directory.FullName }.Concat(parts).ToArray()));
    }
}
