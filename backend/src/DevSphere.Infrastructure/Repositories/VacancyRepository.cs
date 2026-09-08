using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class VacancyRepository
{
    private readonly DevSphereDbContext _context;

    public VacancyRepository(DevSphereDbContext context)
    {
        _context = context;
    }

    public async Task<Vacancy?> GetByIdAsync(Guid id)
    {
        return await _context.Vacancies
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<Vacancy>> GetMineAsync(
        string employerId)
    {
        return await _context.Vacancies
            .AsNoTracking()
            .Where(x => x.EmployerId == employerId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vacancy>> GetOpenAsync(
        string? query,
        string? location,
        int page,
        int pageSize)
    {
        var vacancies = _context.Vacancies
            .AsNoTracking()
            .Where(x =>
                x.LifecycleStatus == VacancyLifecycleStatus.Published &&
                x.IsOpen);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim();

            vacancies = vacancies.Where(x =>
                x.Title.Contains(normalizedQuery) ||
                x.Description.Contains(normalizedQuery));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var normalizedLocation = location.Trim();

            vacancies = vacancies.Where(x =>
                x.Location.Contains(normalizedLocation));
        }

        return await vacancies
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> HasApplicationsAsync(
        Guid vacancyId)
    {
        return await _context.JobApplications
            .AsNoTracking()
            .AnyAsync(x => x.VacancyId == vacancyId);
    }

    public async Task<List<RequiredSkill>> GetRequiredSkillsAsync(
        Guid vacancyId)
    {
        return await _context.RequiredSkills
            .AsNoTracking()
            .Where(x => x.VacancyId == vacancyId)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Dictionary<Guid, List<RequiredSkill>>> GetRequiredSkillsAsync(
        IEnumerable<Guid> vacancyIds)
    {
        var ids = vacancyIds
            .Distinct()
            .ToList();

        var skills = await _context.RequiredSkills
            .AsNoTracking()
            .Where(x => ids.Contains(x.VacancyId))
            .OrderBy(x => x.Name)
            .ToListAsync();

        return skills
            .GroupBy(x => x.VacancyId)
            .ToDictionary(
                x => x.Key,
                x => x.ToList());
    }

    public async Task AddAsync(
        Vacancy vacancy,
        IEnumerable<RequiredSkill> requiredSkills)
    {
        await _context.Vacancies.AddAsync(vacancy);

        await _context.RequiredSkills
            .AddRangeAsync(requiredSkills);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(
        Vacancy vacancy)
    {
        _context.Vacancies.Update(vacancy);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(
        Vacancy vacancy,
        IEnumerable<RequiredSkill> requiredSkills)
    {
        var existingSkills = await _context.RequiredSkills
            .Where(x => x.VacancyId == vacancy.Id)
            .ToListAsync();

        _context.RequiredSkills.RemoveRange(existingSkills);

        await _context.RequiredSkills
            .AddRangeAsync(requiredSkills);

        _context.Vacancies.Update(vacancy);

        await _context.SaveChangesAsync();
    }
}
