using DevSphere.Application.DTOs.Candidates;

namespace DevSphere.Application.Interfaces;

public interface IResumeService
{
    Task<ResumeDto?> GetOwnAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<ResumeDto?> AddUploadedVersionAsync(
        string userId,
        Stream content,
        string fileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken cancellationToken = default);

    Task<ResumeDownloadDto?> DownloadOwnVersionAsync(
        string userId,
        Guid versionId,
        CancellationToken cancellationToken = default);
}
