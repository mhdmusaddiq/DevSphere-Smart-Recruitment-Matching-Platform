using DevSphere.Application.DTOs.Candidates;

namespace DevSphere.Application.Interfaces;

public interface ICandidateCareerService
{
    Task<WorkExperienceDto> AddWorkExperienceAsync(WorkExperienceDto request, CancellationToken cancellationToken = default);
    Task<EducationRecordDto> AddEducationAsync(EducationRecordDto request, CancellationToken cancellationToken = default);
    Task<CertificationRecordDto> AddCertificationAsync(CertificationRecordDto request, CancellationToken cancellationToken = default);
    Task<ProjectRecordDto> AddProjectAsync(ProjectRecordDto request, CancellationToken cancellationToken = default);
    Task<LanguageCapabilityDto> AddLanguageAsync(LanguageCapabilityDto request, CancellationToken cancellationToken = default);
}
