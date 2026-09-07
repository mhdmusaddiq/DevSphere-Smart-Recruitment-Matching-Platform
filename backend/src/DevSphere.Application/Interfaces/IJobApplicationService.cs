using DevSphere.Application.DTOs.Application;

namespace DevSphere.Application.Interfaces;

public interface IJobApplicationService
{
    Task<JobApplicationDto> ApplyAsync(
        JobApplicationDto request);

    Task<List<JobApplicationDto>> GetByCandidateAsync(
        string candidateId);

    Task<List<RankedApplicantDto>> GetByVacancyAsync(
        Guid vacancyId,
        string employerId);


    Task<JobApplicationDto> UpdateStatusAsync(
        Guid applicationId,
        string status);

}
