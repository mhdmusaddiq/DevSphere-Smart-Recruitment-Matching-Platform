using DevSphere.Application.DTOs.Contacts;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Contacts;
using DevSphere.Infrastructure.Repositories.Contacts;

namespace DevSphere.Infrastructure.Services.Contacts;

public class ContactRequestService : IContactRequestService
{
    private readonly ContactRequestRepository _repository;


    public ContactRequestService(
        ContactRequestRepository repository)
    {
        _repository = repository;
    }


    public async Task<ContactRequestDto> SendAsync(
        ContactRequestDto request)
    {
        var entity = new ContactRequest
        {
            Id = Guid.NewGuid(),
            EmployerId = request.EmployerId,
            CandidateId = request.CandidateId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };


        await _repository.AddAsync(entity);


        return Map(entity);
    }


    public async Task<List<ContactRequestDto>> GetByCandidateAsync(
        string candidateId)
    {
        var data = await _repository
            .GetByCandidateAsync(candidateId);


        return data.Select(Map).ToList();
    }


    public async Task<List<ContactRequestDto>> GetByEmployerAsync(
        string employerId)
    {
        var data = await _repository
            .GetByEmployerAsync(employerId);


        return data.Select(Map).ToList();
    }


    public async Task<ContactRequestDto> UpdateStatusAsync(
        Guid id,
        string status)
    {
        var request = await _repository.GetAsync(id);


        if (request == null)
        {
            throw new Exception(
                "Contact request not found.");
        }


        if (status != "Accepted" &&
            status != "Declined")
        {
            throw new Exception(
                "Invalid contact status.");
        }


        request.Status = status;
        request.UpdatedAt = DateTime.UtcNow;


        await _repository.UpdateAsync(request);


        return Map(request);
    }


    private static ContactRequestDto Map(
        ContactRequest request)
    {
        return new ContactRequestDto
        {
            Id = request.Id,
            EmployerId = request.EmployerId,
            CandidateId = request.CandidateId,
            Status = request.Status
        };
    }
}