using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class VacancyRepository
{
    private readonly DevSphereDbContext _context;

    public VacancyRepository(
        DevSphereDbContext context)
    {
        _context = context;
    }

    public async Task<Vacancy?> GetByIdAsync(
        Guid id)
    {
        return await _context.Vacancies
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<Vacancy>> GetOpenAsync(
        string? query,
        string? location,
        int page,
        int pageSize)
    {
        var vacancies = _context.Vacancies
            .AsNoTracking()
            .Where(x => x.IsOpen);

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

    public async Task AddAsync(
        Vacancy vacancy)
    {
        await _context.Vacancies.AddAsync(vacancy);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(
        Vacancy vacancy)
    {
        _context.Vacancies.Update(vacancy);

        await _context.SaveChangesAsync();
    }
}