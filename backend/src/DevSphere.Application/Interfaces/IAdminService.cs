using DevSphere.Application.DTOs.Administration;

namespace DevSphere.Application.Interfaces;

public interface IAdminService
{
    Task<IReadOnlyList<AuditEventDto>> GetRecentAuditEventsAsync(int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SystemSettingDto>> GetSystemSettingsAsync(CancellationToken cancellationToken = default);

    Task<AdminUserPageDto> GetUsersAsync(
        string? q,
        string? role,
        bool? isActive,
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

    Task<IReadOnlyList<AdminSkillConceptDto>>
        GetSkillConceptsAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminSkillAliasDto>>
        GetSkillAliasesAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminOccupationConceptDto>>
        GetOccupationConceptsAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default);

    Task<AdminSkillConceptDto?>
        SetSkillConceptStatusAsync(
            Guid conceptId,
            bool isActive,
            CancellationToken cancellationToken = default);

    Task<AdminSkillAliasDto?>
        SetSkillAliasStatusAsync(
            Guid aliasId,
            bool isActive,
            CancellationToken cancellationToken = default);

    Task<AdminOccupationConceptDto?>
        SetOccupationConceptStatusAsync(
            Guid occupationId,
            bool isActive,
            CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminCompanyVerificationDto>>
        GetCompanyVerificationsAsync(
            string? status,
            CancellationToken cancellationToken = default);

    Task<AdminCompanyVerificationDto?>
        GetCompanyVerificationAsync(
            Guid verificationId,
            CancellationToken cancellationToken = default);

    Task<AdminCompanyVerificationDto?>
        ReviewCompanyVerificationAsync(
            string actorUserId,
            Guid verificationId,
            string decision,
            CancellationToken cancellationToken = default);}
