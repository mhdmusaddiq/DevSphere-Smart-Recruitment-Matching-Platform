using DevSphere.Application.DTOs.Notifications;

namespace DevSphere.Application.Interfaces;

public interface INotificationService
{
    Task CreateAsync(
        string userId,
        string message);


    Task<List<NotificationDto>> GetMyNotificationsAsync(
        string userId);


    Task MarkAsReadAsync(
        Guid id);
}