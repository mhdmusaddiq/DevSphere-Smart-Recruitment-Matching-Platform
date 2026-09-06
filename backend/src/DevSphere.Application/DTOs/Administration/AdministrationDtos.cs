namespace DevSphere.Application.DTOs.Administration;

public class AuditEventDto
{
    public Guid Id { get; set; }

    public string? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }
}

public class SystemSettingDto
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsSensitive { get; set; }
}
