namespace DevSphere.Application.DTOs.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}