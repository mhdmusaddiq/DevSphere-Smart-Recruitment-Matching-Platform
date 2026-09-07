using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Skills;
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

    public async Task<CandidateProfile?> GetByUserIdWithSkillsAsync(
        string userId)
    {
        return await _context.CandidateProfiles
            .Include(x => x.Skills)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<CandidateProfile> AddAsync(
        CandidateProfile profile)
    {
        await _context.CandidateProfiles.AddAsync(profile);

        await _context.SaveChangesAsync();

        return profile;
    }

    public async Task<CandidateProfile> UpdateAsync(
        CandidateProfile profile)
    {
        _context.CandidateProfiles.Update(profile);

        await _context.SaveChangesAsync();

        return profile;
    }

    public async Task<Skill> AddSkillAsync(
        CandidateProfile profile,
        Skill skill)
    {
        profile.Skills.Add(skill);

        await _context.SaveChangesAsync();

        return skill;
    }

    public async Task<bool> RemoveSkillAsync(
        CandidateProfile profile,
        Guid skillId)
    {
        var skill = profile.Skills
            .FirstOrDefault(x => x.Id == skillId);

        if (skill == null)
        {
            return false;
        }

        profile.Skills.Remove(skill);
        _context.Skills.Remove(skill);

        await _context.SaveChangesAsync();

        return true;
    }
}
