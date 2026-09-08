using DevSphere.Application.DTOs.Administration;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Identity;
using DevSphere.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Services.Administration;

public class AdminService : IAdminService
{
    private readonly AdminRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(
        AdminRepository repository,
        UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<AuditEventDto>>
        GetRecentAuditEventsAsync(
            int take,
            CancellationToken cancellationToken = default)
    {
        var events = await _repository
            .GetRecentAuditEventsAsync(
                Math.Clamp(take, 1, 200),
                cancellationToken);

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

    public async Task<IReadOnlyList<SystemSettingDto>>
        GetSystemSettingsAsync(
            CancellationToken cancellationToken = default)
    {
        var settings = await _repository
            .GetSystemSettingsAsync(cancellationToken);

        return settings.Select(x => new SystemSettingDto
        {
            Id = x.Id,
            Key = x.Key,
            Value = x.IsSensitive
                ? string.Empty
                : x.Value,
            Description = x.Description,
            IsSensitive = x.IsSensitive
        }).ToList();
    }

    public async Task<AdminUserPageDto> GetUsersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _userManager.Users
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc);

        var totalCount = await query.CountAsync(
            cancellationToken);

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<AdminUserDto>();

        foreach (var user in users)
        {
            items.Add(await MapUserAsync(user));
        }

        return new AdminUserPageDto
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task<AdminAccountDashboardDto>
        GetAccountDashboardAsync(
            CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var activeUsers = users.Count(x => x.IsActive);

        var adminUsers = 0;
        var employerUsers = 0;
        var candidateUsers = 0;

        foreach (var user in users)
        {
            var roles = await _userManager
                .GetRolesAsync(user);

            if (roles.Contains(AppRoles.Admin))
            {
                adminUsers++;
            }

            if (roles.Contains(AppRoles.Employer))
            {
                employerUsers++;
            }

            if (roles.Contains(AppRoles.Candidate))
            {
                candidateUsers++;
            }
        }

        return new AdminAccountDashboardDto
        {
            TotalUsers = users.Count,
            ActiveUsers = activeUsers,
            DisabledUsers = users.Count - activeUsers,
            AdminUsers = adminUsers,
            EmployerUsers = employerUsers,
            JobSeekerUsers = candidateUsers
        };
    }

    public async Task<AdminUserDto?> SetAccountStatusAsync(
        string actorUserId,
        string targetUserId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated administrator is required.");
        }

        if (string.IsNullOrWhiteSpace(targetUserId))
        {
            throw new ArgumentException(
                "Target user is required.");
        }

        var target = await _userManager
            .FindByIdAsync(targetUserId);

        if (target == null)
        {
            return null;
        }

        if (!isActive &&
            string.Equals(
                actorUserId,
                targetUserId,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Administrators cannot disable their own account.");
        }

        var targetRoles = await _userManager
            .GetRolesAsync(target);

        if (!isActive &&
            target.IsActive &&
            targetRoles.Contains(AppRoles.Admin))
        {
            var admins = await _userManager
                .GetUsersInRoleAsync(AppRoles.Admin);

            var otherActiveAdminExists = admins.Any(x =>
                x.IsActive &&
                !string.Equals(
                    x.Id,
                    target.Id,
                    StringComparison.Ordinal));

            if (!otherActiveAdminExists)
            {
                throw new InvalidOperationException(
                    "The last active administrator cannot be disabled.");
            }
        }

        target.IsActive = isActive;

        var result = await _userManager
            .UpdateAsync(target);

        if (!result.Succeeded)
        {
            var message = string.Join(
                "; ",
                result.Errors.Select(x => x.Description));

            throw new InvalidOperationException(message);
        }

        return await MapUserAsync(target);
    }

    private async Task<AdminUserDto> MapUserAsync(
        ApplicationUser user)
    {
        var roles = await _userManager
            .GetRolesAsync(user);

        var internalRole = roles.FirstOrDefault()
            ?? string.Empty;

        return new AdminUserDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName,
            Role = ToPublicRole(internalRole),
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }

    private static string ToPublicRole(
        string internalRole)
    {
        return string.Equals(
            internalRole,
            AppRoles.Candidate,
            StringComparison.OrdinalIgnoreCase)
            ? "JobSeeker"
            : internalRole;
    }
}
