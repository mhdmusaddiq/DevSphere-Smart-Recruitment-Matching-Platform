using DevSphere.Application.DTOs.Candidates;

namespace DevSphere.Application.Interfaces;

public interface ICandidateCareerService
{
    Task<IReadOnlyCollection<WorkExperienceDto>> GetWorkExperiencesAsync(string userId, CancellationToken cancellationToken = default);
    Task<WorkExperienceDto?> AddWorkExperienceAsync(string userId, WorkExperienceDto request, CancellationToken cancellationToken = default);
    Task<WorkExperienceDto?> UpdateWorkExperienceAsync(string userId, Guid id, WorkExperienceDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteWorkExperienceAsync(string userId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EducationRecordDto>> GetEducationAsync(string userId, CancellationToken cancellationToken = default);
    Task<EducationRecordDto?> AddEducationAsync(string userId, EducationRecordDto request, CancellationToken cancellationToken = default);
    Task<EducationRecordDto?> UpdateEducationAsync(string userId, Guid id, EducationRecordDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteEducationAsync(string userId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CertificationRecordDto>> GetCertificationsAsync(string userId, CancellationToken cancellationToken = default);
    Task<CertificationRecordDto?> AddCertificationAsync(string userId, CertificationRecordDto request, CancellationToken cancellationToken = default);
    Task<CertificationRecordDto?> UpdateCertificationAsync(string userId, Guid id, CertificationRecordDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCertificationAsync(string userId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ProjectRecordDto>> GetProjectsAsync(string userId, CancellationToken cancellationToken = default);
    Task<ProjectRecordDto?> AddProjectAsync(string userId, ProjectRecordDto request, CancellationToken cancellationToken = default);
    Task<ProjectRecordDto?> UpdateProjectAsync(string userId, Guid id, ProjectRecordDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteProjectAsync(string userId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LanguageCapabilityDto>> GetLanguagesAsync(string userId, CancellationToken cancellationToken = default);
    Task<LanguageCapabilityDto?> AddLanguageAsync(string userId, LanguageCapabilityDto request, CancellationToken cancellationToken = default);
    Task<LanguageCapabilityDto?> UpdateLanguageAsync(string userId, Guid id, LanguageCapabilityDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteLanguageAsync(string userId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LicenceRegistrationDto>> GetLicencesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<LicenceRegistrationDto?> AddLicenceAsync(
        string userId,
        LicenceRegistrationDto request,
        CancellationToken cancellationToken = default);

    Task<LicenceRegistrationDto?> UpdateLicenceAsync(
        string userId,
        Guid id,
        LicenceRegistrationDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteLicenceAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default);
}
