using DevSphere.Domain.Entities.Employers;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class CompanyTrustRepository
{
    private readonly DevSphereDbContext _context;

    public CompanyTrustRepository(
        DevSphereDbContext context)
    {
        _context = context;
    }

    public async Task<EmployerProfile?> GetEmployerProfileByUserIdAsync(
        string employerUserId)
    {
        return await _context.EmployerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == employerUserId);
    }

    public async Task<CompanyMembership?> GetMembershipAsync(
        Guid companyId,
        string employerUserId)
    {
        return await _context.CompanyMemberships
            .FirstOrDefaultAsync(x =>
                x.CompanyId == companyId &&
                x.EmployerUserId == employerUserId);
    }

    public async Task<List<CompanyMembership>> GetMembershipsAsync(
        string employerUserId)
    {
        return await _context.CompanyMemberships
            .AsNoTracking()
            .Where(x =>
                x.EmployerUserId == employerUserId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<CompanyProfile?> GetCompanyAsync(
        Guid companyId)
    {
        return await _context.CompanyProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == companyId);
    }

    public async Task<CompanyProfile?> GetCompanyForUpdateAsync(
        Guid companyId)
    {
        return await _context.CompanyProfiles
            .FirstOrDefaultAsync(x => x.Id == companyId);
    }

    public async Task<Dictionary<Guid, CompanyProfile>> GetCompaniesAsync(
        IEnumerable<Guid> companyIds)
    {
        var ids = companyIds
            .Distinct()
            .ToList();

        var companies = await _context.CompanyProfiles
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        return companies.ToDictionary(
            x => x.Id,
            x => x);
    }

    public async Task<CompanyVerification?> GetLatestVerificationAsync(
        Guid companyId)
    {
        return await _context.CompanyVerifications
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.SubmittedAtUtc)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<Guid, CompanyVerification>>
        GetLatestVerificationsAsync(
            IEnumerable<Guid> companyIds)
    {
        var ids = companyIds
            .Distinct()
            .ToList();

        var rows = await _context.CompanyVerifications
            .AsNoTracking()
            .Where(x =>
                x.CompanyId.HasValue &&
                ids.Contains(x.CompanyId.Value))
            .OrderByDescending(x => x.SubmittedAtUtc)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();

        return rows
            .GroupBy(x => x.CompanyId!.Value)
            .ToDictionary(
                x => x.Key,
                x => x.First());
    }

    public async Task AddCompanyAsync(
        CompanyProfile company,
        CompanyMembership membership)
    {
        await _context.CompanyProfiles
            .AddAsync(company);

        await _context.CompanyMemberships
            .AddAsync(membership);

        await _context.SaveChangesAsync();
    }

    public async Task AddVerificationAsync(
        CompanyVerification verification)
    {
        await _context.CompanyVerifications
            .AddAsync(verification);

        await _context.SaveChangesAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
