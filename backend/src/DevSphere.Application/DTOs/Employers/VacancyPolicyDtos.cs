using DevSphere.Domain.Enums;

namespace DevSphere.Application.DTOs.Employers;

public class MatchingPolicyRevisionDto
{
    public Guid Id { get; set; }
    public Guid VacancyId { get; set; }
    public int RevisionNumber { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsMateriallyLocked { get; set; }
    public DateTime? MateriallyLockedAtUtc { get; set; }
}

public class FamilyPolicyRequest
{
    public RequirementFamily RequirementFamily { get; set; }
    public RequirementImportance FamilyImportance { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsScored { get; set; } = true;
}

public class FamilyPolicyDto
{
    public Guid Id { get; set; }
    public Guid MatchingPolicyRevisionId { get; set; }
    public RequirementFamily RequirementFamily { get; set; }
    public RequirementImportance FamilyImportance { get; set; }
    public bool IsActive { get; set; }
    public bool IsScored { get; set; }
}

public class VacancyRequirementRequest
{
    public Guid FamilyPolicyId { get; set; }
    public RequirementFamily RequirementFamily { get; set; }
    public RequirementMode Mode { get; set; }
    public RequirementImportance Importance { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsScored { get; set; } = true;

    public string Description { get; set; } = string.Empty;

    public Guid? SkillConceptId { get; set; }
    public string? CanonicalTargetKey { get; set; }

    public int? RequiredMonths { get; set; }
    public string? RequiredValue { get; set; }
    public string? AcceptedValuesJson { get; set; }

    public bool IsRegulatoryGate { get; set; }
    public bool RequiresVerification { get; set; }

    public string? QuestionText { get; set; }
    public string? ExpectedAnswer { get; set; }

    public int DisplayOrder { get; set; }
}

public class VacancyRequirementPolicyDto
{
    public Guid Id { get; set; }
    public Guid VacancyId { get; set; }
    public Guid MatchingPolicyRevisionId { get; set; }
    public Guid FamilyPolicyId { get; set; }
    public Guid? AlternativeSetId { get; set; }

    public RequirementFamily RequirementFamily { get; set; }
    public RequirementMode Mode { get; set; }
    public RequirementImportance Importance { get; set; }

    public bool IsActive { get; set; }
    public bool IsScored { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? CanonicalTargetKey { get; set; }

    public bool IsRegulatoryGate { get; set; }
    public bool RequiresVerification { get; set; }

    public string? QuestionText { get; set; }
    public int DisplayOrder { get; set; }
}

public class AlternativeSetRequest
{
    public Guid FamilyPolicyId { get; set; }

    public AlternativeSetType SetType { get; set; }

    public int? MinimumSatisfiedCount { get; set; }

    public RequirementMode Mode { get; set; }

    public RequirementImportance Importance { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsScored { get; set; } = true;

    public int DisplayOrder { get; set; }

    public List<Guid> MemberRequirementIds { get; set; } = new();
}

public class AlternativeSetDto
{
    public Guid Id { get; set; }
    public Guid MatchingPolicyRevisionId { get; set; }
    public Guid FamilyPolicyId { get; set; }

    public AlternativeSetType SetType { get; set; }
    public int? MinimumSatisfiedCount { get; set; }

    public RequirementMode Mode { get; set; }
    public RequirementImportance Importance { get; set; }

    public bool IsActive { get; set; }
    public bool IsScored { get; set; }

    public int DisplayOrder { get; set; }

    public List<Guid> MemberRequirementIds { get; set; } = new();
}
