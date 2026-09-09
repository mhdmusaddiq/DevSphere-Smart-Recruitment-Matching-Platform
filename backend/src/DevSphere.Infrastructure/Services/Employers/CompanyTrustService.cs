using DevSphere.Application.DTOs.Employers;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Employers;

public class CompanyTrustService : ICompanyTrustService
{
    private readonly CompanyTrustRepository _repository;

    public CompanyTrustService(
        CompanyTrustRepository repository)
    {
        _repository = repository;
    }

    public async Task<CompanyProfileDto> CreateCompanyAsync(
        string employerUserId,
        CreateCompanyProfileRequest request)
    {
        ValidateEmployerUserId(employerUserId);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Company name is required.");
        }

        var company = new CompanyProfile
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Website = request.Website?.Trim(),
            Location = request.Location?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var membership = new CompanyMembership
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            EmployerUserId = employerUserId,
            Status = CompanyMembershipStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddCompanyAsync(
            company,
            membership);

        return MapToDto(
            company,
            membership,
            null);
    }

    public async Task<IEnumerable<CompanyProfileDto>> GetMineAsync(
        string employerUserId)
    {
        ValidateEmployerUserId(employerUserId);

        var memberships = await _repository
            .GetMembershipsAsync(employerUserId);

        if (memberships.Count == 0)
        {
            return Array.Empty<CompanyProfileDto>();
        }

        var companyIds = memberships
            .Select(x => x.CompanyId)
            .ToList();

        var companies = await _repository
            .GetCompaniesAsync(companyIds);

        var verifications = await _repository
            .GetLatestVerificationsAsync(companyIds);

        return memberships
            .Where(x => companies.ContainsKey(x.CompanyId))
            .Select(membership =>
                MapToDto(
                    companies[membership.CompanyId],
                    membership,
                    verifications.TryGetValue(
                        membership.CompanyId,
                        out var verification)
                        ? verification
                        : null))
            .ToList();
    }

    public async Task<CompanyVerificationResultDto>
        SubmitVerificationAsync(
            string employerUserId,
            Guid companyId,
            SubmitCompanyVerificationRequest request)
    {
        ValidateEmployerUserId(employerUserId);

        if (string.IsNullOrWhiteSpace(
            request.EvidenceStorageKey))
        {
            throw new ArgumentException(
                "Verification evidence is required.");
        }

        var membership = await _repository
            .GetMembershipAsync(
                companyId,
                employerUserId);

        if (membership == null)
        {
            throw new UnauthorizedAccessException(
                "You are not a member of this company.");
        }

        if (membership.Status ==
            CompanyMembershipStatus.Revoked)
        {
            throw new InvalidOperationException(
                "Revoked company memberships cannot submit verification.");
        }

        var company = await _repository
            .GetCompanyAsync(companyId);

        if (company == null)
        {
            throw new KeyNotFoundException(
                "Company not found.");
        }

        var employerProfile = await _repository
            .GetEmployerProfileByUserIdAsync(
                employerUserId);

        if (employerProfile == null)
        {
            throw new InvalidOperationException(
                "Employer profile is required before company verification.");
        }

        var currentVerification = await _repository
            .GetLatestVerificationAsync(companyId);

        if (currentVerification != null &&
            string.Equals(
                currentVerification.Status,
                CompanyVerificationStatus.Verified.ToString(),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Company is already verified.");
        }

        if (currentVerification != null &&
            string.Equals(
                currentVerification.Status,
                CompanyVerificationStatus.PendingReview.ToString(),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Company verification is already pending review.");
        }

        var verification = new CompanyVerification
        {
            Id = Guid.NewGuid(),

            CompanyId = companyId,

            // Derived from authenticated user relationship.
            EmployerProfileId = employerProfile.Id,

            Status =
                CompanyVerificationStatus.PendingReview.ToString(),

            EvidenceStorageKey =
                request.EvidenceStorageKey.Trim(),

            Notes =
                request.Notes?.Trim() ?? string.Empty,

            SubmittedAtUtc = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _repository
            .AddVerificationAsync(verification);

        return new CompanyVerificationResultDto
        {
            Id = verification.Id,
            CompanyId = companyId,
            Status = verification.Status,
            SubmittedAtUtc =
                verification.SubmittedAtUtc
        };
    }

    private static CompanyProfileDto MapToDto(
        CompanyProfile company,
        CompanyMembership membership,
        CompanyVerification? verification)
    {
        return new CompanyProfileDto
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description,
            Website = company.Website,
            Location = company.Location,

            MembershipStatus =
                membership.Status.ToString(),

            VerificationStatus =
                verification?.Status ??
                CompanyVerificationStatus.Draft.ToString()
        };
    }

    private static void ValidateEmployerUserId(
        string employerUserId)
    {
        if (string.IsNullOrWhiteSpace(
            employerUserId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated employer is required.");
        }
    }
}
