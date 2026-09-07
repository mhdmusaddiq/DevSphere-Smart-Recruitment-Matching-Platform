using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Services.Profile;
using Xunit;

namespace DevSphere.Tests.Applications;

public class ApplicationStatusWorkflowTests
{
    [Theory]
    [InlineData(ApplicationStatus.Applied, ApplicationStatus.UnderReview)]
    [InlineData(ApplicationStatus.Applied, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.UnderReview, ApplicationStatus.Shortlisted)]
    [InlineData(ApplicationStatus.UnderReview, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Shortlisted, ApplicationStatus.Selected)]
    [InlineData(ApplicationStatus.Shortlisted, ApplicationStatus.Rejected)]
    public void ValidTransitions_ShouldBeAccepted(
        ApplicationStatus current,
        ApplicationStatus next)
    {
        Assert.True(
            JobApplicationService.IsValidTransition(
                current,
                next));
    }

    [Theory]
    [InlineData(ApplicationStatus.Applied, ApplicationStatus.Selected)]
    [InlineData(ApplicationStatus.Applied, ApplicationStatus.Shortlisted)]
    [InlineData(ApplicationStatus.UnderReview, ApplicationStatus.Selected)]
    [InlineData(ApplicationStatus.Shortlisted, ApplicationStatus.UnderReview)]
    public void InvalidTransitions_ShouldBeRejected(
        ApplicationStatus current,
        ApplicationStatus next)
    {
        Assert.False(
            JobApplicationService.IsValidTransition(
                current,
                next));
    }

    [Theory]
    [InlineData(ApplicationStatus.Applied)]
    [InlineData(ApplicationStatus.UnderReview)]
    [InlineData(ApplicationStatus.Shortlisted)]
    [InlineData(ApplicationStatus.Selected)]
    [InlineData(ApplicationStatus.Rejected)]
    public void SameStatus_ShouldBeRejected(
        ApplicationStatus status)
    {
        Assert.False(
            JobApplicationService.IsValidTransition(
                status,
                status));
    }

    [Theory]
    [InlineData(ApplicationStatus.Selected, ApplicationStatus.Applied)]
    [InlineData(ApplicationStatus.Selected, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.Applied)]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.UnderReview)]
    public void TerminalStatuses_ShouldRejectFurtherTransitions(
        ApplicationStatus current,
        ApplicationStatus next)
    {
        Assert.False(
            JobApplicationService.IsValidTransition(
                current,
                next));
    }
}
