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

public class AdminUserDto
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}

public class AdminUserPageDto
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public IReadOnlyList<AdminUserDto> Items { get; set; }
        = Array.Empty<AdminUserDto>();
}

public class AdminAccountDashboardDto
{
    public int TotalUsers { get; set; }

    public int ActiveUsers { get; set; }

    public int DisabledUsers { get; set; }

    public int AdminUsers { get; set; }

    public int EmployerUsers { get; set; }

    public int JobSeekerUsers { get; set; }
}

public class UpdateAccountStatusRequest
{
    public bool IsActive { get; set; }
}
