using DevSphere.Domain.Entities.Administration;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Entities.Taxonomy;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class AdminRepository
{
    private readonly DevSphereDbContext _context;

    public AdminRepository(DevSphereDbContext context)
    {
        _context = context;
    }

    public Task<List<AuditEvent>> GetRecentAuditEventsAsync(int take, CancellationToken cancellationToken = default)
    {
        return _context.AuditEvents
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<List<SystemSetting>> GetSystemSettingsAsync(CancellationToken cancellationToken = default)
    {
        return _context.SystemSettings
            .OrderBy(x => x.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<ApplicationUser> Users, int TotalCount)>
        GetUsersAsync(
            string? q,
            string? internalRole,
            bool? isActive,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        IQueryable<ApplicationUser> query =
            _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                (x.Email != null && x.Email.Contains(term)) ||
                x.DisplayName.Contains(term) ||
                (x.UserName != null && x.UserName.Contains(term)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(internalRole))
        {
            query = query.Where(user =>
                _context.UserRoles.Any(userRole =>
                    userRole.UserId == user.Id &&
                    _context.Roles.Any(role =>
                        role.Id == userRole.RoleId &&
                        role.Name == internalRole)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }

    public Task<List<SkillConcept>>
        GetSkillConceptsAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default)
    {
        IQueryable<SkillConcept> query =
            _context.SkillConcepts
                .AsNoTracking()
                .Include(x => x.Aliases);

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<List<SkillAlias>>
        GetSkillAliasesAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default)
    {
        IQueryable<SkillAlias> query =
            _context.SkillAliases
                .AsNoTracking()
                .Include(x => x.SkillConcept);

        if (!includeInactive)
        {
            query = query.Where(
                x =>
                    x.IsActive &&
                    x.SkillConcept.IsActive);
        }

        return query
            .OrderBy(x => x.Alias)
            .ToListAsync(cancellationToken);
    }

    public Task<List<OccupationConcept>>
        GetOccupationConceptsAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default)
    {
        IQueryable<OccupationConcept> query =
            _context.OccupationConcepts
                .AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<SkillConcept?>
        SetSkillConceptStatusAsync(
            Guid conceptId,
            bool isActive,
            CancellationToken cancellationToken = default)
    {
        var concept =
            await _context.SkillConcepts
                .Include(x => x.Aliases)
                .FirstOrDefaultAsync(
                    x => x.Id == conceptId,
                    cancellationToken);

        if (concept == null)
        {
            return null;
        }

        concept.IsActive = isActive;

        await _context.SaveChangesAsync(
            cancellationToken);

        return concept;
    }

    public async Task<OccupationConcept?>
        SetOccupationConceptStatusAsync(
            Guid occupationId,
            bool isActive,
            CancellationToken cancellationToken = default)
    {
        var occupation =
            await _context.OccupationConcepts
                .FirstOrDefaultAsync(
                    x => x.Id == occupationId,
                    cancellationToken);

        if (occupation == null)
        {
            return null;
        }

        occupation.IsActive = isActive;

        await _context.SaveChangesAsync(
            cancellationToken);

        return occupation;
    }

    public async Task<SkillAlias?> SetSkillAliasStatusAsync(
        Guid aliasId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var alias = await _context.SkillAliases
            .Include(x => x.SkillConcept)
            .FirstOrDefaultAsync(x => x.Id == aliasId, cancellationToken);

        if (alias == null)
        {
            return null;
        }

        alias.IsActive = isActive;
        await _context.SaveChangesAsync(cancellationToken);
        return alias;
    }
    public Task<List<CompanyVerification>>
        GetCompanyVerificationsAsync(
            string status,
            CancellationToken cancellationToken = default)
    {
        return _context.CompanyVerifications
            .AsNoTracking()
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.SubmittedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<CompanyVerification?>
        GetCompanyVerificationAsync(
            Guid verificationId,
            CancellationToken cancellationToken = default)
    {
        return _context.CompanyVerifications
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == verificationId,
                cancellationToken);
    }

    public Task<CompanyVerification?>
        GetCompanyVerificationForUpdateAsync(
            Guid verificationId,
            CancellationToken cancellationToken = default)
    {
        return _context.CompanyVerifications
            .FirstOrDefaultAsync(
                x => x.Id == verificationId,
                cancellationToken);
    }

    public async Task<Dictionary<Guid, string>>
        GetCompanyNamesAsync(
            IEnumerable<Guid> companyIds,
            CancellationToken cancellationToken = default)
    {
        var ids =
            companyIds
                .Distinct()
                .ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        return await _context.CompanyProfiles
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(
                x => x.Id,
                x => x.Name,
                cancellationToken);
    }

    public async Task SaveCompanyVerificationReviewAsync(
        CompanyVerification verification,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (verification.CompanyId.HasValue)
        {
            var memberships = await _context.CompanyMemberships
                .Where(x =>
                    x.CompanyId == verification.CompanyId.Value &&
                    x.Status != CompanyMembershipStatus.Revoked)
                .ToListAsync(cancellationToken);

            var membershipStatus = verification.Status switch
            {
                nameof(CompanyVerificationStatus.Verified) =>
                    CompanyMembershipStatus.Verified,
                nameof(CompanyVerificationStatus.Rejected) =>
                    CompanyMembershipStatus.Rejected,
                _ => CompanyMembershipStatus.Pending
            };

            foreach (var membership in memberships)
            {
                membership.Status = membershipStatus;
                membership.UpdatedAt = DateTime.UtcNow;
            }
        }

        _context.AuditEvents.Add(
            new AuditEvent
            {
                Id = Guid.NewGuid(),
                UserId = actorUserId,
                Action = "CompanyVerificationReviewed",
                EntityName = "CompanyVerification",
                EntityId = verification.Id.ToString(),
                Details =
                    $"Status changed to {verification.Status}.",
                OccurredAtUtc = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

        await _context.SaveChangesAsync(
            cancellationToken);
    }}
