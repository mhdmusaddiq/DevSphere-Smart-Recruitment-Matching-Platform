using DevSphere.Domain.Entities.Resume;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class ResumeRepository
{
    private readonly DevSphereDbContext _context;

    public ResumeRepository(DevSphereDbContext context)
    {
        _context = context;
    }

    public Task<Resume?> GetByCandidateProfileIdAsync(Guid candidateProfileId, CancellationToken cancellationToken = default)
    {
        return _context.Resumes
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.CandidateProfileId == candidateProfileId, cancellationToken);
    }

    public async Task AddAsync(Resume resume, CancellationToken cancellationToken = default)
    {
        await _context.Resumes.AddAsync(resume, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
