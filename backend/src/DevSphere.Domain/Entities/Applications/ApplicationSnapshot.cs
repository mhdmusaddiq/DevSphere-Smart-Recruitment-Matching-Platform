namespace DevSphere.Domain.Entities.Applications;

public class ApplicationSnapshot : BaseEntity
{
    public Guid JobApplicationId { get; set; }

    public Guid? ResumeVersionId { get; set; }

    public string CandidateSnapshotJson { get; set; } = string.Empty;

    public string VacancySnapshotJson { get; set; } = string.Empty;

    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;
}
