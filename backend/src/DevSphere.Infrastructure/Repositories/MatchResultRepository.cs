using DevSphere.Domain.Entities.Matching;
using DevSphere.Infrastructure.Data;

namespace DevSphere.Infrastructure.Repositories;

public class MatchResultRepository
{
    private readonly DevSphereDbContext _context;

    public MatchResultRepository(DevSphereDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(MatchResult result, CancellationToken cancellationToken = default)
    {
        await _context.MatchResults.AddAsync(result, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
