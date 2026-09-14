using DevSphere.Application.DTOs.Candidates;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Career;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Candidates;

public class CandidateCareerService : ICandidateCareerService
{
    private readonly CandidateCareerRepository _repository;
    private readonly CandidateProfileRepository _profileRepository;

    public CandidateCareerService(
        CandidateCareerRepository repository,
        CandidateProfileRepository profileRepository)
    {
        _repository = repository;
        _profileRepository = profileRepository;
    }

    public async Task<IReadOnlyCollection<WorkExperienceDto>> GetWorkExperiencesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return Array.Empty<WorkExperienceDto>();

        var records = await _repository
            .GetByCandidateProfileIdAsync<WorkExperience>(
                profile.Id,
                cancellationToken);

        return records
            .OrderByDescending(x => x.StartDate)
            .Select(MapWorkExperience)
            .ToList();
    }

    public async Task<WorkExperienceDto?> AddWorkExperienceAsync(
        string userId,
        WorkExperienceDto request,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(request.StartDate, request.EndDate);

        if (string.IsNullOrWhiteSpace(request.JobTitle))
            throw new ArgumentException("Job title is required.");

        if (string.IsNullOrWhiteSpace(request.CompanyName))
            throw new ArgumentException("Company name is required.");

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = new WorkExperience
        {
            Id = Guid.NewGuid(),

            // Never trust CandidateProfileId from client.
            CandidateProfileId = profile.Id,

            JobTitle = request.JobTitle.Trim(),
            CompanyName = request.CompanyName.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Description = request.Description?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);

        return MapWorkExperience(entity);
    }

    public async Task<WorkExperienceDto?> UpdateWorkExperienceAsync(
        string userId,
        Guid id,
        WorkExperienceDto request,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(request.StartDate, request.EndDate);

        if (string.IsNullOrWhiteSpace(request.JobTitle))
            throw new ArgumentException("Job title is required.");

        if (string.IsNullOrWhiteSpace(request.CompanyName))
            throw new ArgumentException("Company name is required.");

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = await _repository
            .GetByIdAsync<WorkExperience>(id, cancellationToken);

        if (entity == null || entity.CandidateProfileId != profile.Id)
            return null;

        entity.JobTitle = request.JobTitle.Trim();
        entity.CompanyName = request.CompanyName.Trim();
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);

        return MapWorkExperience(entity);
    }

    public async Task<bool> DeleteWorkExperienceAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return false;

        var entity = await _repository
            .GetByIdAsync<WorkExperience>(id, cancellationToken);

        if (entity == null || entity.CandidateProfileId != profile.Id)
            return false;

        await _repository.DeleteAsync(entity, cancellationToken);

        return true;
    }

    public async Task<IReadOnlyCollection<EducationRecordDto>> GetEducationAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return Array.Empty<EducationRecordDto>();

        var records = await _repository
            .GetByCandidateProfileIdAsync<EducationRecord>(
                profile.Id,
                cancellationToken);

        return records
            .OrderByDescending(x => x.StartDate)
            .Select(MapEducation)
            .ToList();
    }

    public async Task<EducationRecordDto?> AddEducationAsync(
        string userId,
        EducationRecordDto request,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(request.StartDate, request.EndDate);

        if (string.IsNullOrWhiteSpace(request.Institution))
            throw new ArgumentException("Institution is required.");

        if (string.IsNullOrWhiteSpace(request.Qualification))
            throw new ArgumentException("Qualification is required.");

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = new EducationRecord
        {
            Id = Guid.NewGuid(),

            // Never trust CandidateProfileId from client.
            CandidateProfileId = profile.Id,

            Institution = request.Institution.Trim(),
            Qualification = request.Qualification.Trim(),
            FieldOfStudy = request.FieldOfStudy?.Trim() ?? string.Empty,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);

        return MapEducation(entity);
    }

    public async Task<EducationRecordDto?> UpdateEducationAsync(
        string userId,
        Guid id,
        EducationRecordDto request,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(request.StartDate, request.EndDate);

        if (string.IsNullOrWhiteSpace(request.Institution))
            throw new ArgumentException("Institution is required.");

        if (string.IsNullOrWhiteSpace(request.Qualification))
            throw new ArgumentException("Qualification is required.");

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = await _repository
            .GetByIdAsync<EducationRecord>(id, cancellationToken);

        if (entity == null || entity.CandidateProfileId != profile.Id)
            return null;

        entity.Institution = request.Institution.Trim();
        entity.Qualification = request.Qualification.Trim();
        entity.FieldOfStudy = request.FieldOfStudy?.Trim() ?? string.Empty;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);

        return MapEducation(entity);
    }

    public async Task<bool> DeleteEducationAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return false;

        var entity = await _repository
            .GetByIdAsync<EducationRecord>(id, cancellationToken);

        if (entity == null || entity.CandidateProfileId != profile.Id)
            return false;

        await _repository.DeleteAsync(entity, cancellationToken);

        return true;
    }

    public async Task<IReadOnlyCollection<CertificationRecordDto>> GetCertificationsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return Array.Empty<CertificationRecordDto>();

        var records = await _repository
            .GetByCandidateProfileIdAsync<CertificationRecord>(
                profile.Id,
                cancellationToken);

        return records
            .OrderByDescending(x => x.IssuedOn)
            .Select(MapCertification)
            .ToList();
    }

    public async Task<CertificationRecordDto?> AddCertificationAsync(
        string userId,
        CertificationRecordDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateCertification(request);

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = new CertificationRecord
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = profile.Id,
            Name = request.Name.Trim(),
            Issuer = request.Issuer.Trim(),
            IssuedOn = request.IssuedOn,
            ExpiresOn = request.ExpiresOn,
            CredentialUrl = request.CredentialUrl?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);

        return MapCertification(entity);
    }

    public async Task<CertificationRecordDto?> UpdateCertificationAsync(
        string userId,
        Guid id,
        CertificationRecordDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateCertification(request);

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = await _repository
            .GetByIdAsync<CertificationRecord>(id, cancellationToken);

        if (entity == null || entity.CandidateProfileId != profile.Id)
            return null;

        entity.Name = request.Name.Trim();
        entity.Issuer = request.Issuer.Trim();
        entity.IssuedOn = request.IssuedOn;
        entity.ExpiresOn = request.ExpiresOn;
        entity.CredentialUrl = request.CredentialUrl?.Trim() ?? string.Empty;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);

        return MapCertification(entity);
    }

    public async Task<bool> DeleteCertificationAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return false;

        var entity = await _repository
            .GetByIdAsync<CertificationRecord>(id, cancellationToken);

        if (entity == null || entity.CandidateProfileId != profile.Id)
            return false;

        await _repository.DeleteAsync(entity, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyCollection<ProjectRecordDto>> GetProjectsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return Array.Empty<ProjectRecordDto>();

        var records = await _repository
            .GetByCandidateProfileIdAsync<ProjectRecord>(
                profile.Id,
                cancellationToken);

        return records
            .OrderByDescending(x => x.CreatedAt)
            .Select(MapProject)
            .ToList();
    }

    public async Task<ProjectRecordDto?> AddProjectAsync(
        string userId,
        ProjectRecordDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateProject(request);

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = new ProjectRecord
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = profile.Id,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            ProjectUrl = request.ProjectUrl?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);

        return MapProject(entity);
    }

    public async Task<ProjectRecordDto?> UpdateProjectAsync(
        string userId,
        Guid id,
        ProjectRecordDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateProject(request);

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = await _repository
            .GetByIdAsync<ProjectRecord>(id, cancellationToken);

        if (entity == null || entity.CandidateProfileId != profile.Id)
            return null;

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.ProjectUrl = request.ProjectUrl?.Trim() ?? string.Empty;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);

        return MapProject(entity);
    }

    public async Task<bool> DeleteProjectAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return false;

        var entity = await _repository
            .GetByIdAsync<ProjectRecord>(id, cancellationToken);

        if (entity == null || entity.CandidateProfileId != profile.Id)
            return false;

        await _repository.DeleteAsync(entity, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyCollection<LanguageCapabilityDto>> GetLanguagesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return Array.Empty<LanguageCapabilityDto>();

        var records = await _repository
            .GetByCandidateProfileIdAsync<LanguageCapability>(
                profile.Id,
                cancellationToken);

        return records
            .OrderBy(x => x.Language)
            .Select(MapLanguage)
            .ToList();
    }

    public async Task<LanguageCapabilityDto?> AddLanguageAsync(
        string userId,
        LanguageCapabilityDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateLanguage(request);

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = new LanguageCapability
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = profile.Id,
            Language = request.Language.Trim(),
            Proficiency = request.Proficiency.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);

        return MapLanguage(entity);
    }

    public async Task<LanguageCapabilityDto?> UpdateLanguageAsync(
        string userId,
        Guid id,
        LanguageCapabilityDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateLanguage(request);

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = await _repository
            .GetByIdAsync<LanguageCapability>(id, cancellationToken);

        if (entity == null || entity.CandidateProfileId != profile.Id)
            return null;

        entity.Language = request.Language.Trim();
        entity.Proficiency = request.Proficiency.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);

        return MapLanguage(entity);
    }

    public async Task<bool> DeleteLanguageAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return false;

        var entity = await _repository
            .GetByIdAsync<LanguageCapability>(id, cancellationToken);

        if (entity == null || entity.CandidateProfileId != profile.Id)
            return false;

        await _repository.DeleteAsync(entity, cancellationToken);
        return true;
    }

    private static void ValidateCertification(CertificationRecordDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Certification name is required.");

        if (string.IsNullOrWhiteSpace(request.Issuer))
            throw new ArgumentException("Certification issuer is required.");

        if (request.ExpiresOn.HasValue &&
            request.ExpiresOn.Value < request.IssuedOn)
        {
            throw new ArgumentException(
                "Certification expiry date cannot be earlier than issue date.");
        }
    }

    private static void ValidateProject(ProjectRecordDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Project name is required.");
    }

    private static void ValidateLanguage(LanguageCapabilityDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Language))
            throw new ArgumentException("Language is required.");

        if (string.IsNullOrWhiteSpace(request.Proficiency))
            throw new ArgumentException("Language proficiency is required.");
    }

    private static CertificationRecordDto MapCertification(
        CertificationRecord entity) =>
        new()
        {
            Id = entity.Id,
            CandidateProfileId = entity.CandidateProfileId,
            Name = entity.Name,
            Issuer = entity.Issuer,
            IssuedOn = entity.IssuedOn,
            ExpiresOn = entity.ExpiresOn,
            CredentialUrl = entity.CredentialUrl
        };

    private static ProjectRecordDto MapProject(
        ProjectRecord entity) =>
        new()
        {
            Id = entity.Id,
            CandidateProfileId = entity.CandidateProfileId,
            Name = entity.Name,
            Description = entity.Description,
            ProjectUrl = entity.ProjectUrl
        };

    private static LanguageCapabilityDto MapLanguage(
        LanguageCapability entity) =>
        new()
        {
            Id = entity.Id,
            CandidateProfileId = entity.CandidateProfileId,
            Language = entity.Language,
            Proficiency = entity.Proficiency
        };
    private static void ValidatePeriod(
        DateOnly startDate,
        DateOnly? endDate)
    {
        if (endDate.HasValue && endDate.Value < startDate)
        {
            throw new ArgumentException(
                "End date cannot be earlier than start date.");
        }
    }

    private static WorkExperienceDto MapWorkExperience(
        WorkExperience entity)
    {
        return new WorkExperienceDto
        {
            Id = entity.Id,
            CandidateProfileId = entity.CandidateProfileId,
            JobTitle = entity.JobTitle,
            CompanyName = entity.CompanyName,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Description = entity.Description
        };
    }

    private static EducationRecordDto MapEducation(
        EducationRecord entity)
    {
        return new EducationRecordDto
        {
            Id = entity.Id,
            CandidateProfileId = entity.CandidateProfileId,
            Institution = entity.Institution,
            Qualification = entity.Qualification,
            FieldOfStudy = entity.FieldOfStudy,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate
        };
    }

    public async Task<IReadOnlyCollection<LicenceRegistrationDto>> GetLicencesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return Array.Empty<LicenceRegistrationDto>();

        var records = await _repository
            .GetByCandidateProfileIdAsync<LicenceRegistration>(
                profile.Id,
                cancellationToken);

        return records
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Identifier)
            .Select(MapLicence)
            .ToList();
    }

    public async Task<LicenceRegistrationDto?> AddLicenceAsync(
        string userId,
        LicenceRegistrationDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateLicence(request);

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = new LicenceRegistration
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = profile.Id,
            Type = request.Type.Trim(),
            Class = request.Class?.Trim() ?? string.Empty,
            Issuer = request.Issuer.Trim(),
            Identifier = request.Identifier.Trim(),
            IssuedOn = request.IssuedOn,
            ExpiresOn = request.ExpiresOn,
            Status = NormalizeLicenceStatus(request.Status),
            VerificationStatus =
                NormalizeVerificationStatus(request.VerificationStatus),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);

        return MapLicence(entity);
    }

    public async Task<LicenceRegistrationDto?> UpdateLicenceAsync(
        string userId,
        Guid id,
        LicenceRegistrationDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateLicence(request);

        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return null;

        var entity = await _repository
            .GetByIdAsync<LicenceRegistration>(
                id,
                cancellationToken);

        if (entity == null ||
            entity.CandidateProfileId != profile.Id)
        {
            return null;
        }

        entity.Type = request.Type.Trim();
        entity.Class = request.Class?.Trim() ?? string.Empty;
        entity.Issuer = request.Issuer.Trim();
        entity.Identifier = request.Identifier.Trim();
        entity.IssuedOn = request.IssuedOn;
        entity.ExpiresOn = request.ExpiresOn;
        entity.Status = NormalizeLicenceStatus(request.Status);
        entity.VerificationStatus =
            NormalizeVerificationStatus(request.VerificationStatus);
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);

        return MapLicence(entity);
    }

    public async Task<bool> DeleteLicenceAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            return false;

        var entity = await _repository
            .GetByIdAsync<LicenceRegistration>(
                id,
                cancellationToken);

        if (entity == null ||
            entity.CandidateProfileId != profile.Id)
        {
            return false;
        }

        await _repository.DeleteAsync(
            entity,
            cancellationToken);

        return true;
    }

    private static void ValidateLicence(
        LicenceRegistrationDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Type))
            throw new ArgumentException(
                "Licence or registration type is required.");

        if (string.IsNullOrWhiteSpace(request.Issuer))
            throw new ArgumentException(
                "Licence or registration issuer is required.");

        if (string.IsNullOrWhiteSpace(request.Identifier))
            throw new ArgumentException(
                "Licence or registration identifier is required.");

        if (request.ExpiresOn.HasValue &&
            request.ExpiresOn.Value < request.IssuedOn)
        {
            throw new ArgumentException(
                "Licence expiry date cannot be earlier than issue date.");
        }

        _ = NormalizeLicenceStatus(request.Status);
        _ = NormalizeVerificationStatus(
            request.VerificationStatus);
    }

    private static string NormalizeLicenceStatus(string? status)
    {
        var value = status?.Trim();

        if (string.Equals(
            value,
            "Valid",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Valid";
        }

        if (string.Equals(
            value,
            "Expired",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Expired";
        }

        if (string.Equals(
            value,
            "Suspended",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Suspended";
        }

        if (string.Equals(
            value,
            "Revoked",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Revoked";
        }

        throw new ArgumentException(
            "Licence status must be Valid, Expired, Suspended or Revoked.");
    }

    private static string NormalizeVerificationStatus(
        string? status)
    {
        var value = status?.Trim();

        if (string.Equals(
            value,
            "Pending",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Pending";
        }

        if (string.Equals(
            value,
            "Verified",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Verified";
        }

        if (string.Equals(
            value,
            "Rejected",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Rejected";
        }

        throw new ArgumentException(
            "Verification status must be Pending, Verified or Rejected.");
    }

    private static LicenceRegistrationDto MapLicence(
        LicenceRegistration entity)
    {
        return new LicenceRegistrationDto
        {
            Id = entity.Id,
            CandidateProfileId = entity.CandidateProfileId,
            Type = entity.Type,
            Class = entity.Class,
            Issuer = entity.Issuer,
            Identifier = entity.Identifier,
            IssuedOn = entity.IssuedOn,
            ExpiresOn = entity.ExpiresOn,
            Status = entity.Status,
            VerificationStatus = entity.VerificationStatus
        };
    }
}
