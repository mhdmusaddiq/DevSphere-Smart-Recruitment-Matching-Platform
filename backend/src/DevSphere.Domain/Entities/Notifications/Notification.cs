namespace DevSphere.Domain.Entities.Notifications;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}