using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class MatchingRepository
{
    private readonly DevSphereDbContext _context;

    public MatchingRepository(DevSphereDbContext context)
    {
        _context = context;
    }

    public virtual async Task<CandidateProfile?> GetCandidateAsync(
        string userId)
    {
        return await _context.CandidateProfiles
            .Include(x => x.Skills)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public virtual async Task<Vacancy?> GetVacancyAsync(
        string vacancyId)
    {
        if (!Guid.TryParse(vacancyId, out var id))
        {
            return null;
        }

        return await _context.Vacancies
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public virtual async Task<List<RequiredSkill>> GetRequiredSkillsAsync(
        Guid vacancyId)
    {
        return await _context.RequiredSkills
            .Where(x => x.VacancyId == vacancyId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }
}
