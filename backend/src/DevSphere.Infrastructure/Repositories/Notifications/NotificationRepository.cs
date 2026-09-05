using DevSphere.Domain.Entities.Notifications;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories.Notifications;

public class NotificationRepository
{
    private readonly DevSphereDbContext _context;

    public NotificationRepository(
        DevSphereDbContext context)
    {
        _context = context;
    }


    public async Task AddAsync(
        Notification notification)
    {
        await _context.Notifications.AddAsync(notification);

        await _context.SaveChangesAsync();
    }


    public async Task<List<Notification>> GetByUserAsync(
        string userId)
    {
        return await _context.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }


    public async Task MarkAsReadAsync(
        Guid id)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id);

        if (notification == null)
        {
            return;
        }

        notification.IsRead = true;

        await _context.SaveChangesAsync();
    }
}