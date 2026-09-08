using DevSphere.Application.DTOs.Administration;

namespace DevSphere.Application.Interfaces;

public interface IAdminService
{
    Task<IReadOnlyList<AuditEventDto>> GetRecentAuditEventsAsync(int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SystemSettingDto>> GetSystemSettingsAsync(CancellationToken cancellationToken = default);

    Task<AdminUserPageDto> GetUsersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<AdminAccountDashboardDto> GetAccountDashboardAsync(
        CancellationToken cancellationToken = default);

    Task<AdminUserDto?> SetAccountStatusAsync(
        string actorUserId,
        string targetUserId,
        bool isActive,
        CancellationToken cancellationToken = default);
}
