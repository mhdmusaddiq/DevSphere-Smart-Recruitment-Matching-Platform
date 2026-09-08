namespace DevSphere.Domain.Entities.EmployerWorkflow;

public class InterviewSlot : BaseEntity
{
    public Guid InterviewId { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public DateTime EndsAtUtc { get; set; }

    public string LocationOrMeetingUrl { get; set; } = string.Empty;
}
