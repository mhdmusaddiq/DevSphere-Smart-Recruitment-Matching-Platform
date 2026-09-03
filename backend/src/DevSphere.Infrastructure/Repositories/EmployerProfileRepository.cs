using DevSphere.Domain.Entities.Employers;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class EmployerProfileRepository
{
    private readonly DevSphereDbContext _context;

    public EmployerProfileRepository(
        DevSphereDbContext context)
    {
        _context = context;
    }


    public async Task<EmployerProfile?> GetByUserIdAsync(
        string userId)
    {
        return await _context.EmployerProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }


    public async Task AddAsync(
        EmployerProfile profile)
    {
        await _context.EmployerProfiles
            .AddAsync(profile);

        await _context.SaveChangesAsync();
    }


    public async Task UpdateAsync(
        EmployerProfile profile)
    {
        _context.EmployerProfiles.Update(profile);

        await _context.SaveChangesAsync();
    }
}