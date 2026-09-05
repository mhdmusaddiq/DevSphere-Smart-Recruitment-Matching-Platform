using DevSphere.Application.DTOs.Contacts;

namespace DevSphere.Application.Interfaces;

public interface IContactRequestService
{
    Task<ContactRequestDto> SendAsync(
        ContactRequestDto request);


    Task<List<ContactRequestDto>> GetByCandidateAsync(
        string candidateId);


    Task<List<ContactRequestDto>> GetByEmployerAsync(
        string employerId);


    Task<ContactRequestDto> UpdateStatusAsync(
        Guid id,
        string status);
}