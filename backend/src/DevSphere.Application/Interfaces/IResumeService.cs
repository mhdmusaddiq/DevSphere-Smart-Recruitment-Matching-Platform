using DevSphere.Application.DTOs.Candidates;

namespace DevSphere.Application.Interfaces;

public interface IResumeService
{
    Task<ResumeDto?> GetAsync(Guid candidateProfileId, CancellationToken cancellationToken = default);

    Task<ResumeDto> AddVersionAsync(Guid candidateProfileId, ResumeVersionDto version, CancellationToken cancellationToken = default);
}
