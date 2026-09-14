using DevSphere.Application.DTOs.Application;

namespace DevSphere.Application.Interfaces;

public interface IJobApplicationService
{
    Task<JobApplicationDto> ApplyAsync(
        string candidateId,
        ApplyApplicationRequest request);

    Task<ApplyDecisionDto> GetApplyDecisionAsync(
        string candidateId,
        Guid vacancyId);

    Task<List<JobApplicationDto>> GetByCandidateAsync(
        string candidateId);

    Task<JobApplicationDto?> GetCandidateApplicationAsync(
        Guid applicationId,
        string candidateId);

    Task<EmployerApplicationDetailDto> GetEmployerApplicationAsync(
        Guid applicationId,
        string employerId);

    Task<List<RankedApplicantDto>> GetByVacancyAsync(
        Guid vacancyId,
        string employerId);


    Task<JobApplicationDto> UpdateStatusAsync(
        Guid applicationId,
        string status,
        string changedByUserId);

    Task<JobApplicationDto> WithdrawAsync(
        Guid applicationId,
        string candidateId);

}
