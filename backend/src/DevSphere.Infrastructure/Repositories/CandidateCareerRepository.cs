using DevSphere.Domain.Entities;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class CandidateCareerRepository
{
    private readonly DevSphereDbContext _context;

    public CandidateCareerRepository(DevSphereDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetByCandidateProfileIdAsync<TEntity>(
        Guid candidateProfileId,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        return await _context.Set<TEntity>()
            .Where(x => EF.Property<Guid>(x, "CandidateProfileId") == candidateProfileId)
            .ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync<TEntity>(
        Guid id,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        return await _context.Set<TEntity>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        _context.Set<TEntity>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        _context.Set<TEntity>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
