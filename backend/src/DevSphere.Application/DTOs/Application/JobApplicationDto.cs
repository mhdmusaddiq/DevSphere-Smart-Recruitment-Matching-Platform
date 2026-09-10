namespace DevSphere.Application.DTOs.Application;

public class JobApplicationDto
{
    public Guid Id { get; set; }

    public string CandidateId { get; set; } = string.Empty;

    public Guid VacancyId { get; set; }

    public string VacancyTitle { get; set; } = string.Empty;

    public string VacancyLocation { get; set; } = string.Empty;

    public string WorkMode { get; set; } = string.Empty;

    public Guid? CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; }

    public string Status { get; set; } = string.Empty;

    public string FrozenAssessmentStatus { get; set; } = string.Empty;

    public decimal? DisplayCompatibility { get; set; }

    public string Eligibility { get; set; } = string.Empty;

    public Guid? ResumeVersionId { get; set; }

    public DateTime? CapturedAtUtc { get; set; }

    public ApplyDecisionDto? ApplyDecision { get; set; }
}

public class ApplyApplicationRequest
{
    public Guid VacancyId { get; set; }

    public bool BaselineAcknowledged { get; set; }
}

public class EmployerApplicationDetailDto
{
    public Guid ApplicationId { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public string CandidateDisplayName { get; set; } = string.Empty;
    public Guid VacancyId { get; set; }
    public string VacancyTitle { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public MatchAssessmentStatus AssessmentStatus { get; set; }
    public decimal? DisplayCompatibility { get; set; }
    public MatchEligibilityStatus Eligibility { get; set; }
    public string? EligibilityReason { get; set; }
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public List<string> MissingInputs { get; set; } = new();
    public List<MatchFamilyResultDto> Families { get; set; } = new();
    public Guid? ResumeVersionId { get; set; }
    public DateTime? CapturedAtUtc { get; set; }
}

public class ApplyDecisionDto
{
    public bool CanSubmit { get; set; }

    public bool RequiresBaselineAcknowledgement { get; set; }

    public string PrimaryCode { get; set; } = string.Empty;

    public List<ApplyDecisionReasonDto> Reasons { get; set; } = new();

    public DateTime EvaluatedAtUtc { get; set; }

    public Guid VacancyId { get; set; }

    public Guid? MatchingPolicyRevisionId { get; set; }

    public int? MatchingPolicyRevisionNumber { get; set; }

    public Guid? ResumeVersionId { get; set; }
}

public class ApplyDecisionReasonDto
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string TargetCta { get; set; } = string.Empty;
}
