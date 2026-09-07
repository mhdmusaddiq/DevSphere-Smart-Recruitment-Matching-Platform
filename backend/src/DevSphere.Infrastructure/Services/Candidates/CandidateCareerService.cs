using DevSphere.Application.DTOs.Candidates;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Career;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Candidates;

public class CandidateCareerService : ICandidateCareerService
{
    private readonly CandidateCareerRepository _repository;

    public CandidateCareerService(CandidateCareerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<WorkExperienceDto>> GetWorkExperiencesAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken = default)
    {
        var records = await _repository
            .GetByCandidateProfileIdAsync<WorkExperience>(
                candidateProfileId,
                cancellationToken);

        return records
            .OrderByDescending(x => x.StartDate)
            .Select(MapWorkExperience)
            .ToList();
    }

    public async Task<WorkExperienceDto> AddWorkExperienceAsync(
        WorkExperienceDto request,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(request.StartDate, request.EndDate);

        if (string.IsNullOrWhiteSpace(request.JobTitle))
            throw new ArgumentException("Job title is required.");

        if (string.IsNullOrWhiteSpace(request.CompanyName))
            throw new ArgumentException("Company name is required.");

        var entity = new WorkExperience
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = request.CandidateProfileId,
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
        Guid id,
        WorkExperienceDto request,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(request.StartDate, request.EndDate);

        if (string.IsNullOrWhiteSpace(request.JobTitle))
            throw new ArgumentException("Job title is required.");

        if (string.IsNullOrWhiteSpace(request.CompanyName))
            throw new ArgumentException("Company name is required.");

        var entity = await _repository
            .GetByIdAsync<WorkExperience>(id, cancellationToken);

        if (entity == null)
            return null;

        entity.JobTitle = request.JobTitle.Trim();
        entity.CompanyName = request.CompanyName.Trim();
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Description = request.Description?.Trim() ?? string.Empty;

        await _repository.UpdateAsync(entity, cancellationToken);

        return MapWorkExperience(entity);
    }

    public async Task<bool> DeleteWorkExperienceAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository
            .GetByIdAsync<WorkExperience>(id, cancellationToken);

        if (entity == null)
            return false;

        await _repository.DeleteAsync(entity, cancellationToken);

        return true;
    }

    public async Task<IReadOnlyCollection<EducationRecordDto>> GetEducationAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken = default)
    {
        var records = await _repository
            .GetByCandidateProfileIdAsync<EducationRecord>(
                candidateProfileId,
                cancellationToken);

        return records
            .OrderByDescending(x => x.StartDate)
            .Select(MapEducation)
            .ToList();
    }

    public async Task<EducationRecordDto> AddEducationAsync(
        EducationRecordDto request,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(request.StartDate, request.EndDate);

        if (string.IsNullOrWhiteSpace(request.Institution))
            throw new ArgumentException("Institution is required.");

        if (string.IsNullOrWhiteSpace(request.Qualification))
            throw new ArgumentException("Qualification is required.");

        var entity = new EducationRecord
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = request.CandidateProfileId,
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
        Guid id,
        EducationRecordDto request,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(request.StartDate, request.EndDate);

        if (string.IsNullOrWhiteSpace(request.Institution))
            throw new ArgumentException("Institution is required.");

        if (string.IsNullOrWhiteSpace(request.Qualification))
            throw new ArgumentException("Qualification is required.");

        var entity = await _repository
            .GetByIdAsync<EducationRecord>(id, cancellationToken);

        if (entity == null)
            return null;

        entity.Institution = request.Institution.Trim();
        entity.Qualification = request.Qualification.Trim();
        entity.FieldOfStudy = request.FieldOfStudy?.Trim() ?? string.Empty;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;

        await _repository.UpdateAsync(entity, cancellationToken);

        return MapEducation(entity);
    }

    public async Task<bool> DeleteEducationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository
            .GetByIdAsync<EducationRecord>(id, cancellationToken);

        if (entity == null)
            return false;

        await _repository.DeleteAsync(entity, cancellationToken);

        return true;
    }

    public async Task<CertificationRecordDto> AddCertificationAsync(
        CertificationRecordDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = new CertificationRecord
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = request.CandidateProfileId,
            Name = request.Name,
            Issuer = request.Issuer,
            IssuedOn = request.IssuedOn,
            ExpiresOn = request.ExpiresOn,
            CredentialUrl = request.CredentialUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);
        request.Id = entity.Id;

        return request;
    }

    public async Task<ProjectRecordDto> AddProjectAsync(
        ProjectRecordDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = new ProjectRecord
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = request.CandidateProfileId,
            Name = request.Name,
            Description = request.Description,
            ProjectUrl = request.ProjectUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);
        request.Id = entity.Id;

        return request;
    }

    public async Task<LanguageCapabilityDto> AddLanguageAsync(
        LanguageCapabilityDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = new LanguageCapability
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = request.CandidateProfileId,
            Language = request.Language,
            Proficiency = request.Proficiency,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);
        request.Id = entity.Id;

        return request;
    }

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
}
