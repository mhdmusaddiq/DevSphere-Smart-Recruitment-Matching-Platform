using DevSphere.Application.DTOs.Contacts;
using DevSphere.Domain.Entities.Contacts;
using Xunit;

namespace DevSphere.Tests.Contacts;

public class ContactRequestLifecycleTests
{
    [Fact]
    public void ContactRequest_Should_Be_ApplicationScoped()
    {
        var applicationId = Guid.NewGuid();

        var request = new ContactRequest
        {
            JobApplicationId = applicationId,
            EmployerId = "employer-1",
            CandidateId = "candidate-1",
            Status = "Pending"
        };

        Assert.Equal(applicationId, request.JobApplicationId);
        Assert.Equal("employer-1", request.EmployerId);
        Assert.Equal("candidate-1", request.CandidateId);
        Assert.Equal("Pending", request.Status);
    }


    [Fact]
    public void ContactRequestDto_Should_Expose_ApplicationRelationship()
    {
        var applicationId = Guid.NewGuid();

        var dto = new ContactRequestDto
        {
            JobApplicationId = applicationId,
            EmployerId = "employer-1",
            CandidateId = "candidate-1",
            Status = "Pending"
        };

        Assert.Equal(applicationId, dto.JobApplicationId);
        Assert.Equal("employer-1", dto.EmployerId);
        Assert.Equal("candidate-1", dto.CandidateId);
        Assert.Equal("Pending", dto.Status);
    }


    [Theory]
    [InlineData("Accepted")]
    [InlineData("Declined")]
    public void CandidateDecision_Should_Use_Final_Status(
        string status)
    {
        var request = new ContactRequest
        {
            JobApplicationId = Guid.NewGuid(),
            EmployerId = "employer-1",
            CandidateId = "candidate-1",
            Status = status
        };

        Assert.NotEqual("Pending", request.Status);
        Assert.Contains(
            request.Status,
            new[] { "Accepted", "Declined" });
    }
}
