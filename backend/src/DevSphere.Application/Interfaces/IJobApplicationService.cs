using DevSphere.Application.DTOs.Application;

namespace DevSphere.Application.Interfaces;

public interface IJobApplicationService
{
    Task<JobApplicationDto> ApplyAsync(
        JobApplicationDto request);

    Task<List<JobApplicationDto>> GetByCandidateAsync(
        string candidateId);
}