using DevSphere.Application.DTOs.Application;

namespace DevSphere.Application.Interfaces;

public interface IApplicationHistoryService
{
    Task<ApplicationSnapshotDto> CreateSnapshotAsync(ApplicationSnapshotDto request, CancellationToken cancellationToken = default);

    Task<ApplicationSnapshotDto?> GetSnapshotAsync(
        Guid jobApplicationId,
        CancellationToken cancellationToken = default);
    Task<ApplicationStatusHistoryDto> RecordStatusAsync(ApplicationStatusHistoryDto request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationStatusHistoryDto>> GetStatusHistoryAsync(Guid jobApplicationId, CancellationToken cancellationToken = default);
}
