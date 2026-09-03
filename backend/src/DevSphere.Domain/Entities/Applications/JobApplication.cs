using DevSphere.Domain.Enums;

namespace DevSphere.Domain.Entities.Applications;

public class JobApplication : BaseEntity
{
    public string CandidateId { get; set; } = string.Empty;

    public string VacancyId { get; set; } = string.Empty;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}