using DevSphere.Domain.Entities.Candidates;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class CandidateProfileRepository
{
    private readonly DevSphereDbContext _context;

    public CandidateProfileRepository(
        DevSphereDbContext context)
    {
        _context = context;
    }


    public async Task<CandidateProfile?> GetByUserIdAsync(
        string userId)
    {
        return await _context.CandidateProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }


    public async Task<CandidateProfile> AddAsync(
        CandidateProfile profile)
    {
        await _context.CandidateProfiles.AddAsync(profile);

        await _context.SaveChangesAsync();

        return profile;
    }
}