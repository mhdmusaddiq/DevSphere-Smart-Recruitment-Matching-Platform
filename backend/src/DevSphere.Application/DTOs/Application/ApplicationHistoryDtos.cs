namespace DevSphere.Application.DTOs.Application;

public class ApplicationSnapshotDto
{
    public Guid Id { get; set; }

    public Guid JobApplicationId { get; set; }

    public Guid? ResumeVersionId { get; set; }

    public Guid? MatchingPolicyRevisionId { get; set; }

    public decimal CompatibilityScore { get; set; }

    public decimal RawCompatibilityScore { get; set; }

    public decimal DisplayCompatibilityScore { get; set; }

    public string CompatibilityStatus { get; set; } = string.Empty;

    public string EligibilityStatus { get; set; } = string.Empty;

    public bool IsEligible { get; set; }

    public string ApplyDecision { get; set; } = string.Empty;

    public string MatchedSkillsJson { get; set; } = string.Empty;

    public string GapSkillsJson { get; set; } = string.Empty;

    public string EvidenceSummaryJson { get; set; } = string.Empty;

    public string CandidateSnapshotJson { get; set; } = string.Empty;

    public string VacancySnapshotJson { get; set; } = string.Empty;

    public DateTime CapturedAtUtc { get; set; }
}

public class ApplicationStatusHistoryDto
{
    public Guid Id { get; set; }

    public Guid JobApplicationId { get; set; }

    public string? PreviousStatus { get; set; }

    public string NewStatus { get; set; } = string.Empty;

    public string ChangedByUserId { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; }

    public string Notes { get; set; } = string.Empty;
}
