namespace DevSphere.Application.DTOs.Application;

public class JobApplicationDto
{
    public Guid Id { get; set; }

    public string CandidateId { get; set; } = string.Empty;

    public Guid VacancyId { get; set; }

    public string Status { get; set; } = string.Empty;

    public ApplyDecisionDto? ApplyDecision { get; set; }
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
