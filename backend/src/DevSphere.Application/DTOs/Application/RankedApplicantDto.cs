namespace DevSphere.Application.DTOs.Application;

public class RankedApplicantDto
{
    public Guid ApplicationId { get; set; }

    public string CandidateId { get; set; } = string.Empty;

    public Guid VacancyId { get; set; }

    public DateTime AppliedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    // Raw unrounded decimal remains ranking authority.
    public decimal? RawCompatibility { get; set; }

    // Display-only compatibility projection.
    public decimal? MatchScore { get; set; }

    public MatchAssessmentStatus AssessmentStatus { get; set; }

    public MatchEligibilityStatus Eligibility { get; set; }

    public string? EligibilityReason { get; set; }

    public decimal? HighTierAggregate { get; set; }

    public decimal? MediumTierAggregate { get; set; }

    public decimal Coverage { get; set; }

    public List<string> MatchedSkills { get; set; } = new();

    public List<string> MissingSkills { get; set; } = new();

    public List<string> MissingInputs { get; set; } = new();

    // Read-only criterion/family evidence used by employer compare.
    public List<MatchFamilyResultDto> Families { get; set; } = new();
}
