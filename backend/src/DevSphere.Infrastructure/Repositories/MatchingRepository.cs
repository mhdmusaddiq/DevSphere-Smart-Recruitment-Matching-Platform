using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class MatchingRepository
{
    private readonly DevSphereDbContext _context;

    public MatchingRepository(
        DevSphereDbContext context)
    {
        _context = context;
    }


    public virtual async Task<CandidateProfile?> GetCandidateAsync(
    string candidateId)
    {
        return await _context.CandidateProfiles
     .Include(x => x.Skills)
     .FirstOrDefaultAsync(x =>
         x.Id.ToString() == candidateId);
    }


    public virtual async Task<Vacancy?> GetVacancyAsync(
    string vacancyId)
    {
        return await _context.Vacancies
            .Include(x => x.RequiredSkills)
            .FirstOrDefaultAsync(x =>
                x.Id.ToString() == vacancyId);
    }
}