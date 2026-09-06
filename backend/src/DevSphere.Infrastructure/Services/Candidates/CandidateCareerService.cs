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

    public async Task<WorkExperienceDto> AddWorkExperienceAsync(WorkExperienceDto request, CancellationToken cancellationToken = default)
    {
        var entity = new WorkExperience
        {
            Id = Guid.NewGuid(), CandidateProfileId = request.CandidateProfileId,
            JobTitle = request.JobTitle, CompanyName = request.CompanyName,
            StartDate = request.StartDate, EndDate = request.EndDate,
            Description = request.Description, CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(entity, cancellationToken);
        request.Id = entity.Id;
        return request;
    }

    public async Task<EducationRecordDto> AddEducationAsync(EducationRecordDto request, CancellationToken cancellationToken = default)
    {
        var entity = new EducationRecord
        {
            Id = Guid.NewGuid(), CandidateProfileId = request.CandidateProfileId,
            Institution = request.Institution, Qualification = request.Qualification,
            FieldOfStudy = request.FieldOfStudy, StartDate = request.StartDate,
            EndDate = request.EndDate, CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(entity, cancellationToken);
        request.Id = entity.Id;
        return request;
    }

    public async Task<CertificationRecordDto> AddCertificationAsync(CertificationRecordDto request, CancellationToken cancellationToken = default)
    {
        var entity = new CertificationRecord
        {
            Id = Guid.NewGuid(), CandidateProfileId = request.CandidateProfileId,
            Name = request.Name, Issuer = request.Issuer, IssuedOn = request.IssuedOn,
            ExpiresOn = request.ExpiresOn, CredentialUrl = request.CredentialUrl,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(entity, cancellationToken);
        request.Id = entity.Id;
        return request;
    }

    public async Task<ProjectRecordDto> AddProjectAsync(ProjectRecordDto request, CancellationToken cancellationToken = default)
    {
        var entity = new ProjectRecord
        {
            Id = Guid.NewGuid(), CandidateProfileId = request.CandidateProfileId,
            Name = request.Name, Description = request.Description,
            ProjectUrl = request.ProjectUrl, CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(entity, cancellationToken);
        request.Id = entity.Id;
        return request;
    }

    public async Task<LanguageCapabilityDto> AddLanguageAsync(LanguageCapabilityDto request, CancellationToken cancellationToken = default)
    {
        var entity = new LanguageCapability
        {
            Id = Guid.NewGuid(), CandidateProfileId = request.CandidateProfileId,
            Language = request.Language, Proficiency = request.Proficiency,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(entity, cancellationToken);
        request.Id = entity.Id;
        return request;
    }
}
