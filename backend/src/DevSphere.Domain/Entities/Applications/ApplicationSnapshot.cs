namespace DevSphere.Domain.Entities.Applications;

public class ApplicationSnapshot : BaseEntity
{
    public Guid JobApplicationId { get; set; }

    public Guid? ResumeVersionId { get; set; }

    public Guid? MatchingPolicyRevisionId { get; set; }

    public decimal? CompatibilityScore { get; set; }

    public decimal? RawCompatibilityScore { get; set; }

    public decimal? DisplayCompatibilityScore { get; set; }

    public decimal? HighTierAggregateScore { get; set; }

    public decimal? MediumTierAggregateScore { get; set; }

    public decimal Coverage { get; set; }

    public string CompatibilityStatus { get; set; } = string.Empty;

    public string EligibilityStatus { get; set; } = string.Empty;

    public string? EligibilityReason { get; set; }

    public bool IsEligible { get; set; }

    public string ApplyDecision { get; set; } = string.Empty;

    public string MatchedSkillsJson { get; set; } = string.Empty;

    public string GapSkillsJson { get; set; } = string.Empty;

    public string EvidenceSummaryJson { get; set; } = string.Empty;

    public string MatchResultJson { get; set; } = string.Empty;

    public string CandidateSnapshotJson { get; set; } = string.Empty;

    public string VacancySnapshotJson { get; set; } = string.Empty;

    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;
}
