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
    Guid vacancyId)
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
        Guid vacancyId,
        string employerId)
    {
        return await _context.JobApplications
            .Include(x => x.Vacancy)
            .Where(x =>
                x.VacancyId == vacancyId &&
                x.Vacancy.EmployerId == employerId)
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
            .Include(x => x.Vacancy)
            .FirstOrDefaultAsync(x => x.Id == id);
    }




    public async Task AddWithSnapshotAsync(
        JobApplication application,
        ApplicationSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            await _context.JobApplications.AddAsync(
                application,
                cancellationToken);

            await _context.ApplicationSnapshots.AddAsync(
                snapshot,
                cancellationToken);

            await _context.ApplicationStatusHistories.AddAsync(
                new ApplicationStatusHistory
                {
                    Id = Guid.NewGuid(),
                    JobApplicationId = application.Id,
                    PreviousStatus = null,
                    NewStatus = application.Status,
                    ChangedByUserId = application.CandidateId,
                    ChangedAtUtc = application.AppliedAt,
                    Notes = "Application submitted.",
                    CreatedAt = application.AppliedAt
                },
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }
}
