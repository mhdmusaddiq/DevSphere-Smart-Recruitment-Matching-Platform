using DevSphere.Domain.Entities.Contacts;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories.Contacts;

public class ContactRequestRepository
{
    private readonly DevSphereDbContext _context;

    public ContactRequestRepository(
        DevSphereDbContext context)
    {
        _context = context;
    }


    public async Task AddAsync(
        ContactRequest request)
    {
        await _context.ContactRequests.AddAsync(request);

        await _context.SaveChangesAsync();
    }


    public async Task<ContactRequest?> GetAsync(
        Guid id)
    {
        return await _context.ContactRequests
            .FirstOrDefaultAsync(x => x.Id == id);
    }


    public async Task<List<ContactRequest>> GetByCandidateAsync(
        string candidateId)
    {
        return await _context.ContactRequests
            .Where(x => x.CandidateId == candidateId)
            .ToListAsync();
    }


    public async Task<List<ContactRequest>> GetByEmployerAsync(
        string employerId)
    {
        return await _context.ContactRequests
            .Where(x => x.EmployerId == employerId)
            .ToListAsync();
    }


    public async Task UpdateAsync(
        ContactRequest request)
    {
        _context.ContactRequests.Update(request);

        await _context.SaveChangesAsync();
    }
}