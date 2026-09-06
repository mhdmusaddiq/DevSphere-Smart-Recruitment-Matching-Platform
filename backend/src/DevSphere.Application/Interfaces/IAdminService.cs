using DevSphere.Application.DTOs.Administration;

namespace DevSphere.Application.Interfaces;

public interface IAdminService
{
    Task<IReadOnlyList<AuditEventDto>> GetRecentAuditEventsAsync(int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SystemSettingDto>> GetSystemSettingsAsync(CancellationToken cancellationToken = default);
}
