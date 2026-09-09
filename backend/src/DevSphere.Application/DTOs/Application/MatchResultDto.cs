namespace DevSphere.Application.DTOs.Application;

public enum MatchAssessmentStatus
{
    Calculated = 1,
    Provisional = 2,
    NotCalculated = 3,
    CalculationFailure = 4
}

public enum MatchCriterionState
{
    Met = 1,
    NotMet = 2,
    NotDemonstrated = 3,
    Incomplete = 4,
    PendingVerification = 5,
    NotApplicable = 6
}

public enum MatchEligibilityStatus
{
    MeetsBaseline = 1,
    PendingVerification = 2,
    IncompleteAssessment = 3,
    DoesNotMeetBaseline = 4
}

public class MatchCriterionResultDto
{
    public Guid? RequirementId { get; set; }

    public Guid? AlternativeSetId { get; set; }

    public string Family { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public string Importance { get; set; } = string.Empty;

    public MatchCriterionState State { get; set; }

    public decimal? Score { get; set; }

    public bool IsRegulatoryGate { get; set; }

    public string Label { get; set; } = string.Empty;
}

public class MatchFamilyResultDto
{
    public string Family { get; set; } = string.Empty;

    public string Importance { get; set; } = string.Empty;

    public decimal? RawScore { get; set; }

    public decimal? DisplayScore { get; set; }

    public List<MatchCriterionResultDto> Criteria { get; set; } = new();
}

public class MatchResultDto
{
    public MatchAssessmentStatus AssessmentStatus { get; set; }

    public MatchEligibilityStatus Eligibility { get; set; }

    public string? EligibilityReason { get; set; }

    // Raw unrounded decimal is the ranking authority.
    public decimal? RawCompatibility { get; set; }

    // UI projection only: one decimal, AwayFromZero.
    public decimal? DisplayCompatibility { get; set; }

    public decimal? HighTierAggregate { get; set; }

    public decimal? MediumTierAggregate { get; set; }

    public decimal Coverage { get; set; }

    public List<string> MatchedSkills { get; set; } = new();

    public List<string> MissingSkills { get; set; } = new();

    public List<string> MissingInputs { get; set; } = new();

    public List<MatchFamilyResultDto> Families { get; set; } = new();
}
