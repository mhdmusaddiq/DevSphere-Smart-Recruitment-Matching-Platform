using DevSphere.Domain.Enums;

namespace DevSphere.Domain.Entities.EmployerWorkflow;

public class Interview : BaseEntity
{
    public Guid JobApplicationId { get; set; }

    public string EmployerUserId { get; set; } = string.Empty;

    public InterviewStatus Status { get; set; }
        = InterviewStatus.Scheduled;

    public string Notes { get; set; } = string.Empty;
}
