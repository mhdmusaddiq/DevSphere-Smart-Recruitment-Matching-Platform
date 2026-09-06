using DevSphere.Domain.Entities.Administration;
using DevSphere.Infrastructure.Data;
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
}
