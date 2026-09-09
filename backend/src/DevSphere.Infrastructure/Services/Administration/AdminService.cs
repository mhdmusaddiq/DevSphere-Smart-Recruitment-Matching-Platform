using DevSphere.Application.DTOs.Administration;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Taxonomy;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Enums;
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


    public async Task<IReadOnlyList<AdminSkillConceptDto>>
        GetSkillConceptsAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default)
    {
        var concepts =
            await _repository.GetSkillConceptsAsync(
                includeInactive,
                cancellationToken);

        return concepts
            .Select(
                x => MapSkillConcept(
                    x,
                    includeInactive))
            .ToList();
    }

    public async Task<IReadOnlyList<AdminSkillAliasDto>>
        GetSkillAliasesAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default)
    {
        var aliases =
            await _repository.GetSkillAliasesAsync(
                includeInactive,
                cancellationToken);

        return aliases
            .Select(MapSkillAlias)
            .ToList();
    }

    public async Task<IReadOnlyList<AdminOccupationConceptDto>>
        GetOccupationConceptsAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default)
    {
        var occupations =
            await _repository.GetOccupationConceptsAsync(
                includeInactive,
                cancellationToken);

        return occupations
            .Select(MapOccupation)
            .ToList();
    }

    public async Task<AdminSkillConceptDto?>
        SetSkillConceptStatusAsync(
            Guid conceptId,
            bool isActive,
            CancellationToken cancellationToken = default)
    {
        var concept =
            await _repository
                .SetSkillConceptStatusAsync(
                    conceptId,
                    isActive,
                    cancellationToken);

        return concept == null
            ? null
            : MapSkillConcept(
                concept,
                true);
    }

    public async Task<AdminOccupationConceptDto?>
        SetOccupationConceptStatusAsync(
            Guid occupationId,
            bool isActive,
            CancellationToken cancellationToken = default)
    {
        var occupation =
            await _repository
                .SetOccupationConceptStatusAsync(
                    occupationId,
                    isActive,
                    cancellationToken);

        return occupation == null
            ? null
            : MapOccupation(occupation);
    }

    private static AdminSkillConceptDto MapSkillConcept(
        SkillConcept concept,
        bool includeInactiveAliases)
    {
        var aliases =
            concept.Aliases
                .Where(
                    x =>
                        includeInactiveAliases ||
                        x.IsActive)
                .OrderBy(x => x.Alias)
                .Select(x => new AdminSkillAliasDto
                {
                    Id = x.Id,
                    SkillConceptId =
                        x.SkillConceptId,
                    SkillConceptName =
                        concept.Name,
                    Alias = x.Alias,
                    IsActive = x.IsActive,
                    SkillConceptIsActive =
                        concept.IsActive
                })
                .ToList();

        return new AdminSkillConceptDto
        {
            Id = concept.Id,
            Name = concept.Name,
            IsActive = concept.IsActive,
            Aliases = aliases
        };
    }

    private static AdminSkillAliasDto MapSkillAlias(
        SkillAlias alias)
    {
        return new AdminSkillAliasDto
        {
            Id = alias.Id,
            SkillConceptId =
                alias.SkillConceptId,
            SkillConceptName =
                alias.SkillConcept.Name,
            Alias = alias.Alias,
            IsActive = alias.IsActive,
            SkillConceptIsActive =
                alias.SkillConcept.IsActive
        };
    }

    private static AdminOccupationConceptDto MapOccupation(
        OccupationConcept occupation)
    {
        return new AdminOccupationConceptDto
        {
            Id = occupation.Id,
            Name = occupation.Name,
            IsActive = occupation.IsActive
        };
    }

    public async Task<IReadOnlyList<AdminCompanyVerificationDto>>
        GetCompanyVerificationsAsync(
            string? status,
            CancellationToken cancellationToken = default)
    {
        var normalizedStatus =
            NormalizeVerificationStatus(
                status);

        var rows =
            await _repository
                .GetCompanyVerificationsAsync(
                    normalizedStatus,
                    cancellationToken);

        var companyIds =
            rows
                .Where(x => x.CompanyId.HasValue)
                .Select(x => x.CompanyId!.Value)
                .ToList();

        var companyNames =
            await _repository
                .GetCompanyNamesAsync(
                    companyIds,
                    cancellationToken);

        return rows
            .Select(
                x => MapCompanyVerification(
                    x,
                    companyNames))
            .ToList();
    }

    public async Task<AdminCompanyVerificationDto?>
        GetCompanyVerificationAsync(
            Guid verificationId,
            CancellationToken cancellationToken = default)
    {
        var verification =
            await _repository
                .GetCompanyVerificationAsync(
                    verificationId,
                    cancellationToken);

        if (verification == null)
        {
            return null;
        }

        var ids =
            verification.CompanyId.HasValue
                ? new[]
                {
                    verification.CompanyId.Value
                }
                : Array.Empty<Guid>();

        var companyNames =
            await _repository
                .GetCompanyNamesAsync(
                    ids,
                    cancellationToken);

        return MapCompanyVerification(
            verification,
            companyNames);
    }

    public async Task<AdminCompanyVerificationDto?>
        ReviewCompanyVerificationAsync(
            string actorUserId,
            Guid verificationId,
            string decision,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(
            actorUserId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated administrator is required.");
        }

        var reviewDecision =
            ParseReviewDecision(decision);

        var verification =
            await _repository
                .GetCompanyVerificationForUpdateAsync(
                    verificationId,
                    cancellationToken);

        if (verification == null)
        {
            return null;
        }

        if (!string.Equals(
            verification.Status,
            CompanyVerificationStatus
                .PendingReview
                .ToString(),
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only pending-review company verifications can be reviewed.");
        }

        verification.Status =
            reviewDecision.ToString();

        verification.ReviewedAtUtc =
            DateTime.UtcNow;

        verification.ReviewedByUserId =
            actorUserId;

        await _repository
            .SaveCompanyVerificationReviewAsync(
                verification,
                actorUserId,
                cancellationToken);

        var ids =
            verification.CompanyId.HasValue
                ? new[]
                {
                    verification.CompanyId.Value
                }
                : Array.Empty<Guid>();

        var companyNames =
            await _repository
                .GetCompanyNamesAsync(
                    ids,
                    cancellationToken);

        return MapCompanyVerification(
            verification,
            companyNames);
    }

    private static string NormalizeVerificationStatus(
        string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return CompanyVerificationStatus
                .PendingReview
                .ToString();
        }

        if (!Enum.TryParse<CompanyVerificationStatus>(
            status,
            true,
            out var parsed))
        {
            throw new ArgumentException(
                "Invalid company verification status.");
        }

        return parsed.ToString();
    }

    private static CompanyVerificationStatus
        ParseReviewDecision(
            string decision)
    {
        if (!Enum.TryParse<CompanyVerificationStatus>(
            decision,
            true,
            out var parsed))
        {
            throw new ArgumentException(
                "Invalid company verification decision.");
        }

        if (parsed !=
                CompanyVerificationStatus.Verified &&
            parsed !=
                CompanyVerificationStatus
                    .NeedsMoreInformation &&
            parsed !=
                CompanyVerificationStatus.Rejected)
        {
            throw new ArgumentException(
                "Review decision must be Verified, NeedsMoreInformation, or Rejected.");
        }

        return parsed;
    }

    private static AdminCompanyVerificationDto
        MapCompanyVerification(
            CompanyVerification verification,
            IReadOnlyDictionary<Guid, string>
                companyNames)
    {
        var companyName = string.Empty;

        if (verification.CompanyId.HasValue)
        {
            companyNames.TryGetValue(
                verification.CompanyId.Value,
                out companyName);
        }

        return new AdminCompanyVerificationDto
        {
            Id = verification.Id,
            CompanyId = verification.CompanyId,
            CompanyName =
                companyName ?? string.Empty,
            Status = verification.Status,
            EvidenceStorageKey =
                verification.EvidenceStorageKey,
            Notes = verification.Notes,
            SubmittedAtUtc =
                verification.SubmittedAtUtc,
            ReviewedAtUtc =
                verification.ReviewedAtUtc,
            ReviewedByUserId =
                verification.ReviewedByUserId
        };
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
