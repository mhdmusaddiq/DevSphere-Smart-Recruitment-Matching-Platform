namespace DevSphere.Domain.Entities.EmployerWorkflow;

public class TalentPoolEntry : BaseEntity
{
    public Guid JobApplicationId { get; set; }

    public string CandidateUserId { get; set; } = string.Empty;

    public string EmployerUserId { get; set; } = string.Empty;

    public bool HasCandidateConsent { get; set; }

    public DateTime? ConsentRecordedAtUtc { get; set; }

    public bool IsActive { get; set; } = true;

    public string Notes { get; set; } = string.Empty;
}
