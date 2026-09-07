using DevSphere.Application.DTOs.Candidates;

namespace DevSphere.Application.Interfaces;

public interface ICandidateCareerService
{
    Task<IReadOnlyCollection<WorkExperienceDto>> GetWorkExperiencesAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken = default);

    Task<WorkExperienceDto> AddWorkExperienceAsync(
        WorkExperienceDto request,
        CancellationToken cancellationToken = default);

    Task<WorkExperienceDto?> UpdateWorkExperienceAsync(
        Guid id,
        WorkExperienceDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteWorkExperienceAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EducationRecordDto>> GetEducationAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken = default);

    Task<EducationRecordDto> AddEducationAsync(
        EducationRecordDto request,
        CancellationToken cancellationToken = default);

    Task<EducationRecordDto?> UpdateEducationAsync(
        Guid id,
        EducationRecordDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteEducationAsync(
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
