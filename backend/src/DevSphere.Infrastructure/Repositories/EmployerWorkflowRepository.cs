using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Entities.EmployerWorkflow;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class EmployerWorkflowRepository
{
    private readonly DevSphereDbContext _context;

    public EmployerWorkflowRepository(
        DevSphereDbContext context)
    {
        _context = context;
    }

    public async Task<JobApplication?>
        GetApplicationWithVacancyAsync(
            Guid applicationId)
    {
        return await _context.JobApplications
            .Include(x => x.Vacancy)
            .FirstOrDefaultAsync(
                x => x.Id == applicationId);
    }

    public async Task<Interview?> GetInterviewAsync(
        Guid interviewId)
    {
        return await _context.Interviews
            .FirstOrDefaultAsync(
                x => x.Id == interviewId);
    }

    public async Task<Offer?> GetOfferAsync(
        Guid offerId)
    {
        return await _context.Offers
            .FirstOrDefaultAsync(
                x => x.Id == offerId);
    }

    public async Task<bool> TalentPoolEntryExistsAsync(
        Guid applicationId,
        string employerUserId)
    {
        return await _context.TalentPoolEntries
            .AsNoTracking()
            .AnyAsync(x =>
                x.JobApplicationId == applicationId &&
                x.EmployerUserId == employerUserId);
    }

    public async Task AddInterviewAsync(
        Interview interview)
    {
        await _context.Interviews.AddAsync(interview);
        await _context.SaveChangesAsync();
    }

    public async Task AddInterviewSlotAsync(
        InterviewSlot slot)
    {
        await _context.InterviewSlots.AddAsync(slot);
        await _context.SaveChangesAsync();
    }

    public async Task AddScorecardAsync(
        Scorecard scorecard)
    {
        await _context.Scorecards.AddAsync(scorecard);
        await _context.SaveChangesAsync();
    }

    public async Task AddOfferAsync(
        Offer offer)
    {
        await _context.Offers.AddAsync(offer);
        await _context.SaveChangesAsync();
    }

    public async Task AddTalentPoolEntryAsync(
        TalentPoolEntry entry)
    {
        await _context.TalentPoolEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<(
        List<Interview> Interviews,
        List<InterviewSlot> Slots,
        List<Scorecard> Scorecards,
        List<Offer> Offers,
        List<TalentPoolEntry> TalentPoolEntries)> GetSummaryAsync(
            Guid applicationId)
    {
        var interviews = await _context.Interviews
            .AsNoTracking()
            .Where(x => x.JobApplicationId == applicationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        var interviewIds = interviews.Select(x => x.Id).ToList();
        var slots = await _context.InterviewSlots
            .AsNoTracking()
            .Where(x => interviewIds.Contains(x.InterviewId))
            .OrderBy(x => x.StartsAtUtc)
            .ToListAsync();
        var scorecards = await _context.Scorecards
            .AsNoTracking()
            .Where(x => x.JobApplicationId == applicationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        var offers = await _context.Offers
            .AsNoTracking()
            .Where(x => x.JobApplicationId == applicationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        var talentPoolEntries = await _context.TalentPoolEntries
            .AsNoTracking()
            .Where(x => x.JobApplicationId == applicationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        return (
            interviews,
            slots,
            scorecards,
            offers,
            talentPoolEntries);
    }
}
