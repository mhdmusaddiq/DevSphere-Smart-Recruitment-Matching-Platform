using DevSphere.Application.DTOs.Notifications;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Notifications;
using DevSphere.Infrastructure.Repositories.Notifications;

namespace DevSphere.Infrastructure.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly NotificationRepository _repository;


    public NotificationService(
        NotificationRepository repository)
    {
        _repository = repository;
    }


    public async Task CreateAsync(
        string userId,
        string message)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Message = message,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };


        await _repository.AddAsync(notification);
    }



    public async Task<List<NotificationDto>> GetMyNotificationsAsync(
        string userId)
    {
        var notifications = await _repository
            .GetByUserAsync(userId);


        return notifications
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToList();
    }



    public async Task MarkAsReadAsync(
        Guid id)
    {
        await _repository.MarkAsReadAsync(id);
    }
}