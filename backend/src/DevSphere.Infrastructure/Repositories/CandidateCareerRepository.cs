using DevSphere.Domain.Entities;
using DevSphere.Infrastructure.Data;

namespace DevSphere.Infrastructure.Repositories;

public class CandidateCareerRepository
{
    private readonly DevSphereDbContext _context;

    public CandidateCareerRepository(DevSphereDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
