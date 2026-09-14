namespace DevSphere.Domain.Entities.EmployerWorkflow;

public class Scorecard : BaseEntity
{
    public Guid JobApplicationId { get; set; }

    public Guid? InterviewId { get; set; }

    public string AssessorEmployerUserId { get; set; } = string.Empty;

    // Human assessment only. This is deliberately separate
    // from the compatibility/matching score.
    public int OverallRating { get; set; }

    public string Notes { get; set; } = string.Empty;
}
