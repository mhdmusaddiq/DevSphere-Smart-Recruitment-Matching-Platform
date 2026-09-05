using DevSphere.Domain.Entities.Applications;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class JobApplicationRepository
{
    private readonly DevSphereDbContext _context;

    public JobApplicationRepository(
        DevSphereDbContext context)
    {
        _context = context;
    }


    public async Task<bool> ExistsAsync(
        string candidateId,
        string vacancyId)
    {
        return await _context.JobApplications
            .AnyAsync(x =>
                x.CandidateId == candidateId &&
                x.VacancyId == vacancyId);
    }


    public async Task AddAsync(
        JobApplication application)
    {
        await _context.JobApplications.AddAsync(application);

        await _context.SaveChangesAsync();
    }


    public async Task<IEnumerable<JobApplication>> GetByCandidateAsync(
        string candidateId)
    {
        return await _context.JobApplications
            .Where(x => x.CandidateId == candidateId)
            .ToListAsync();
    }


    public async Task<IEnumerable<JobApplication>> GetByVacancyAsync(
        string vacancyId)
    {
        return await _context.JobApplications
            .Where(x => x.VacancyId == vacancyId)
            .ToListAsync();
    }

    public async Task<JobApplication?> GetByIdAsync(
    Guid id)
    {
        return await _context.JobApplications
            .FirstOrDefaultAsync(x => x.Id == id);
    }


    public async Task UpdateAsync(
        JobApplication application)
    {
        _context.JobApplications.Update(application);

        await _context.SaveChangesAsync();
    }

    public async Task<JobApplication?> GetWithVacancyAsync(
    Guid id)
    {
        return await _context.JobApplications
            .Include(x => x.VacancyId)
            .FirstOrDefaultAsync(x => x.Id == id);
    }



}