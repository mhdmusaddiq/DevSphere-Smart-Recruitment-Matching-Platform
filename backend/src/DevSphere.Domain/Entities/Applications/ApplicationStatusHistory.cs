using DevSphere.Domain.Enums;

namespace DevSphere.Domain.Entities.Applications;

public class ApplicationStatusHistory : BaseEntity
{
    public Guid JobApplicationId { get; set; }

    public ApplicationStatus? PreviousStatus { get; set; }

    public ApplicationStatus NewStatus { get; set; }

    public string ChangedByUserId { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;

    public string Notes { get; set; } = string.Empty;
}
