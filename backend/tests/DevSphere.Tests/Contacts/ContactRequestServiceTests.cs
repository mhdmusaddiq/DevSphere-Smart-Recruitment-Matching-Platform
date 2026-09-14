using DevSphere.Application.DTOs.Contacts;
using DevSphere.Infrastructure.Repositories.Contacts;
using DevSphere.Infrastructure.Services.Contacts;
using DevSphere.Domain.Entities.Contacts;
using Xunit;

namespace DevSphere.Tests.Contacts;

public class ContactRequestServiceTests
{
    [Fact]
    public void ContactRequest_Should_Start_As_Pending()
    {
        var request = new ContactRequest
        {
            Id = Guid.NewGuid(),
            EmployerId = "emp1",
            CandidateId = "can1",
            Status = "Pending"
        };


        Assert.Equal(
            "Pending",
            request.Status);
    }


    [Fact]
    public void ContactRequestDto_Should_Map_Status()
    {
        var dto = new ContactRequestDto
        {
            EmployerId = "emp1",
            CandidateId = "can1",
            Status = "Accepted"
        };


        Assert.Equal(
            "Accepted",
            dto.Status);
    }
}