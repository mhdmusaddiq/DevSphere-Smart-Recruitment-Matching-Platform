using DevSphere.Application.DTOs.Contacts;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Contacts;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Repositories.Contacts;

namespace DevSphere.Infrastructure.Services.Contacts;

public class ContactRequestService : IContactRequestService
{
    private readonly ContactRequestRepository _repository;
    private readonly JobApplicationRepository _applicationRepository;


    public ContactRequestService(
        ContactRequestRepository repository,
        JobApplicationRepository applicationRepository)
    {
        _repository = repository;
        _applicationRepository = applicationRepository;
    }


    public async Task<ContactRequestDto> SendAsync(
        Guid jobApplicationId,
        string employerId)
    {
        if (jobApplicationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Job application is required.",
                nameof(jobApplicationId));
        }

        if (string.IsNullOrWhiteSpace(employerId))
        {
            throw new ArgumentException(
                "Employer is required.",
                nameof(employerId));
        }

        var application = await _applicationRepository
            .GetWithVacancyAsync(jobApplicationId);

        if (application == null)
        {
            throw new KeyNotFoundException(
                "Job application not found.");
        }

        if (application.Vacancy == null ||
            !string.Equals(
                application.Vacancy.EmployerId,
                employerId,
                StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException(
                "Employer does not own this application.");
        }

        var exists = await _repository
            .ExistsForApplicationAsync(
                jobApplicationId,
                employerId);

        if (exists)
        {
            throw new InvalidOperationException(
                "A contact request already exists for this application.");
        }

        var entity = new ContactRequest
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.Id,
            EmployerId = employerId,
            CandidateId = application.CandidateId,
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
        string status,
        string candidateId)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
        {
            throw new ArgumentException(
                "Candidate is required.",
                nameof(candidateId));
        }

        var request = await _repository
            .GetForCandidateAsync(
                id,
                candidateId);

        if (request == null)
        {
            throw new KeyNotFoundException(
                "Contact request not found.");
        }

        if (!string.Equals(
                request.Status,
                "Pending",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Only pending contact requests can be updated.");
        }

        if (!string.Equals(
                status,
                "Accepted",
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                status,
                "Declined",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Invalid contact status.",
                nameof(status));
        }

        request.Status =
            string.Equals(
                status,
                "Accepted",
                StringComparison.OrdinalIgnoreCase)
                ? "Accepted"
                : "Declined";

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
            JobApplicationId = request.JobApplicationId,
            EmployerId = request.EmployerId,
            CandidateId = request.CandidateId,
            Status = request.Status
        };
    }
}
