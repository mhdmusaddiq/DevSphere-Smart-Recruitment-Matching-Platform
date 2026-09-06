using DevSphere.Domain.Entities.Applications;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class ApplicationHistoryRepository
{
    private readonly DevSphereDbContext _context;

    public ApplicationHistoryRepository(DevSphereDbContext context)
    {
        _context = context;
    }

    public async Task AddSnapshotAsync(ApplicationSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _context.ApplicationSnapshots.AddAsync(snapshot, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddStatusAsync(ApplicationStatusHistory history, CancellationToken cancellationToken = default)
    {
        await _context.ApplicationStatusHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<List<ApplicationStatusHistory>> GetStatusHistoryAsync(Guid jobApplicationId, CancellationToken cancellationToken = default)
    {
        return _context.ApplicationStatusHistories
            .Where(x => x.JobApplicationId == jobApplicationId)
            .OrderBy(x => x.ChangedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
