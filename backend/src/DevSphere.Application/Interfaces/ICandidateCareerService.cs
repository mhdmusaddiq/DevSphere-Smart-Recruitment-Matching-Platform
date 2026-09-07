using DevSphere.Application.DTOs.Candidates;

namespace DevSphere.Application.Interfaces;

public interface ICandidateCareerService
{
    Task<IReadOnlyCollection<WorkExperienceDto>> GetWorkExperiencesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<WorkExperienceDto?> AddWorkExperienceAsync(
        string userId,
        WorkExperienceDto request,
        CancellationToken cancellationToken = default);

    Task<WorkExperienceDto?> UpdateWorkExperienceAsync(
        string userId,
        Guid id,
        WorkExperienceDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteWorkExperienceAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EducationRecordDto>> GetEducationAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<EducationRecordDto?> AddEducationAsync(
        string userId,
        EducationRecordDto request,
        CancellationToken cancellationToken = default);

    Task<EducationRecordDto?> UpdateEducationAsync(
        string userId,
        Guid id,
        EducationRecordDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteEducationAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CertificationRecordDto> AddCertificationAsync(
        CertificationRecordDto request,
        CancellationToken cancellationToken = default);

    Task<ProjectRecordDto> AddProjectAsync(
        ProjectRecordDto request,
        CancellationToken cancellationToken = default);

    Task<LanguageCapabilityDto> AddLanguageAsync(
        LanguageCapabilityDto request,
        CancellationToken cancellationToken = default);
}
