using DevSphere.Application.DTOs.Contacts;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Contacts;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Repositories.Contacts;
using DevSphere.Infrastructure.Data;
using DevSphere.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Services.Contacts;

public class ContactRequestService : IContactRequestService
{
    private readonly ContactRequestRepository _repository;
    private readonly JobApplicationRepository _applicationRepository;
    private readonly DevSphereDbContext _context;


    public ContactRequestService(
        ContactRequestRepository repository,
        JobApplicationRepository applicationRepository,
        DevSphereDbContext context)
    {
        _repository = repository;
        _applicationRepository = applicationRepository;
        _context = context;
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

        return await MapAsync(entity, employerView: true);
    }


    public async Task<List<ContactRequestDto>> GetByCandidateAsync(
        string candidateId)
    {
        var data = await _repository
            .GetByCandidateAsync(candidateId);

        var results = new List<ContactRequestDto>();
        foreach (var request in data)
        {
            results.Add(await MapAsync(request, employerView: false));
        }

        return results;
    }


    public async Task<List<ContactRequestDto>> GetByEmployerAsync(
        string employerId)
    {
        var data = await _repository
            .GetByEmployerAsync(employerId);

        var results = new List<ContactRequestDto>();
        foreach (var request in data)
        {
            results.Add(await MapAsync(request, employerView: true));
        }

        return results;
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

        return await MapAsync(request, employerView: false);
    }

    public async Task<ContactRequestDto> UpdateEmployerStatusAsync(
        Guid id,
        string status,
        string employerId)
    {
        var request = await _repository.GetAsync(id);

        if (request == null || request.EmployerId != employerId)
        {
            throw new KeyNotFoundException(
                "Contact request not found.");
        }

        var normalizedStatus = status.Trim();
        var canCancel = request.Status == "Pending" &&
            normalizedStatus.Equals(
                "Cancelled",
                StringComparison.OrdinalIgnoreCase);
        var canRevoke = request.Status == "Accepted" &&
            normalizedStatus.Equals(
                "Revoked",
                StringComparison.OrdinalIgnoreCase);

        if (!canCancel && !canRevoke)
        {
            throw new InvalidOperationException(
                $"Invalid contact transition from {request.Status} to {status}.");
        }

        request.Status = canCancel ? "Cancelled" : "Revoked";
        request.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(request);

        return await MapAsync(request, employerView: true);
    }


    private async Task<ContactRequestDto> MapAsync(
        ContactRequest request,
        bool employerView)
    {
        var application = await _context.JobApplications
            .AsNoTracking()
            .Include(x => x.Vacancy)
            .FirstOrDefaultAsync(x =>
                x.Id == request.JobApplicationId);

        var candidate = await _context.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserId == request.CandidateId);
        var candidateUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.CandidateId);
        var employerUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.EmployerId);

        var companyId = application?.Vacancy.CompanyId;
        var companyName = string.Empty;
        var companyIsVerified = false;
        var membershipIsVerified = false;

        if (companyId.HasValue)
        {
            companyName = await _context.CompanyProfiles
                .AsNoTracking()
                .Where(x => x.Id == companyId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync() ?? string.Empty;

            membershipIsVerified = await _context.CompanyMemberships
                .AsNoTracking()
                .AnyAsync(x =>
                    x.CompanyId == companyId.Value &&
                    x.EmployerUserId == request.EmployerId &&
                    x.Status == CompanyMembershipStatus.Verified);

            var latestVerification = await _context.CompanyVerifications
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId.Value)
                .OrderByDescending(x => x.SubmittedAtUtc)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => x.Status)
                .FirstOrDefaultAsync();

            companyIsVerified = string.Equals(
                latestVerification,
                CompanyVerificationStatus.Verified.ToString(),
                StringComparison.OrdinalIgnoreCase);
        }

        var canDisclose = employerView &&
            request.Status == "Accepted" &&
            candidateUser?.IsActive == true &&
            employerUser?.IsActive == true &&
            membershipIsVerified &&
            companyIsVerified;

        return Map(
            request,
            application?.Vacancy.Id ?? Guid.Empty,
            application?.Vacancy.Title ?? string.Empty,
            companyId,
            companyName,
            candidate?.FullName ?? string.Empty,
            employerUser?.DisplayName ?? string.Empty,
            canDisclose,
            canDisclose ? candidateUser?.Email : null);
    }

    private static ContactRequestDto Map(
        ContactRequest request)
    {
        return Map(
            request,
            Guid.Empty,
            string.Empty,
            null,
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            null);
    }

    private static ContactRequestDto Map(
        ContactRequest request,
        Guid vacancyId,
        string vacancyTitle,
        Guid? companyId,
        string companyName,
        string candidateDisplayName,
        string employerDisplayName,
        bool canDiscloseContact,
        string? candidateEmail)
    {
        return new ContactRequestDto
        {
            Id = request.Id,
            JobApplicationId = request.JobApplicationId,
            EmployerId = request.EmployerId,
            CandidateId = request.CandidateId,
            Status = request.Status,
            VacancyId = vacancyId,
            VacancyTitle = vacancyTitle,
            CompanyId = companyId,
            CompanyName = companyName,
            CandidateDisplayName = candidateDisplayName,
            EmployerDisplayName = employerDisplayName,
            CanDiscloseContact = canDiscloseContact,
            CandidateEmail = candidateEmail
        };
    }
}
