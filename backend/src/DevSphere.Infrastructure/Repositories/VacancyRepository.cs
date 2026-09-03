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


    public async Task<IEnumerable<Vacancy>> GetOpenAsync()
    {
        return await _context.Vacancies
            .Where(x => x.IsOpen)
            .ToListAsync();
    }


    public async Task AddAsync(
        Vacancy vacancy)
    {
        await _context.Vacancies.AddAsync(vacancy);

        await _context.SaveChangesAsync();
    }
}