using DevSphere.Application.DTOs.Administration;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Administration;

public class AdminService : IAdminService
{
    private readonly AdminRepository _repository;

    public AdminService(AdminRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AuditEventDto>> GetRecentAuditEventsAsync(int take, CancellationToken cancellationToken = default)
    {
        var events = await _repository.GetRecentAuditEventsAsync(Math.Clamp(take, 1, 200), cancellationToken);
        return events.Select(x => new AuditEventDto
        {
            Id = x.Id,
            UserId = x.UserId,
            Action = x.Action,
            EntityName = x.EntityName,
            EntityId = x.EntityId,
            Details = x.Details,
            OccurredAtUtc = x.OccurredAtUtc
        }).ToList();
    }

    public async Task<IReadOnlyList<SystemSettingDto>> GetSystemSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetSystemSettingsAsync(cancellationToken);
        return settings.Select(x => new SystemSettingDto
        {
            Id = x.Id,
            Key = x.Key,
            Value = x.IsSensitive ? string.Empty : x.Value,
            Description = x.Description,
            IsSensitive = x.IsSensitive
        }).ToList();
    }
}
