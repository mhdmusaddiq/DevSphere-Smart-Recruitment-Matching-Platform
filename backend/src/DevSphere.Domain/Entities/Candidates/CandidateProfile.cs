using DevSphere.Domain.Entities.Skills;

namespace DevSphere.Domain.Entities.Candidates;

public class CandidateProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int ExperienceMonths { get; set; }

    public string Education { get; set; } = string.Empty;

    // Structured candidate work preferences used by readiness/matching.
    public string PreferredWorkMode { get; set; } = string.Empty;

    public string PreferredLocation { get; set; } = string.Empty;

    public bool WillingToRelocate { get; set; }

    public string PreferredEmploymentType { get; set; } = string.Empty;

    // Structured availability. Empty values are allowed while profile is incomplete.
    public string AvailabilityStatus { get; set; } = string.Empty;

    public DateOnly? AvailableFrom { get; set; }

    public int? NoticePeriodDays { get; set; }

    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
