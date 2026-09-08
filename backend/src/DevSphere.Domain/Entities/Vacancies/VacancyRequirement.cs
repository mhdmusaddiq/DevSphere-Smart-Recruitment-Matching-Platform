using DevSphere.Domain.Enums;

namespace DevSphere.Domain.Entities.Vacancies;

public class VacancyRequirement : BaseEntity
{
    // Existing BRD relationship retained for compatibility.
    public Guid VacancyId { get; set; }

    // Professional policy revision. Nullable for legacy requirement rows.
    public Guid? MatchingPolicyRevisionId { get; set; }

    // Every professional active criterion belongs to one family policy.
    public Guid? FamilyPolicyId { get; set; }

    // Optional AlternativeSet membership.
    public Guid? AlternativeSetId { get; set; }

    public RequirementFamily RequirementFamily { get; set; }
        = RequirementFamily.Skill;

    public RequirementMode Mode { get; set; }
        = RequirementMode.Mandatory;

    // Criterion-level importance participates inside the family.
    // Family importance remains owned only by FamilyPolicy.
    public RequirementImportance Importance { get; set; }
        = RequirementImportance.Medium;

    public bool IsActive { get; set; } = true;

    public bool IsScored { get; set; } = true;

    // Existing BRD field retained.
    public string Description { get; set; } = string.Empty;

    // Compatibility projection for the old mandatory flag.
    public bool IsMandatory { get; set; }

    // Canonical target metadata. SkillConceptId is populated when the
    // canonical knowledge concept is available.
    public Guid? SkillConceptId { get; set; }

    public string? CanonicalTargetKey { get; set; }

    // Structured values used by deterministic family evaluators.
    public int? RequiredMonths { get; set; }

    public string? RequiredValue { get; set; }

    public string? AcceptedValuesJson { get; set; }

    // Regulatory gates are metadata on a criterion, not a separate family.
    public bool IsRegulatoryGate { get; set; }

    public bool RequiresVerification { get; set; }

    // StructuredQuestion model. Subjective/free-text questions should
    // remain informational rather than scored.
    public string? QuestionText { get; set; }

    public string? ExpectedAnswer { get; set; }

    public int DisplayOrder { get; set; }
}
