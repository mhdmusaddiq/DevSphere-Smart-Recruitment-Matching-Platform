using DevSphere.Domain.Enums;

namespace DevSphere.Domain.Entities.Vacancies;

public class AlternativeSet : BaseEntity
{
    public Guid MatchingPolicyRevisionId { get; set; }

    public Guid FamilyPolicyId { get; set; }

    public AlternativeSetType SetType { get; set; }

    public int? MinimumSatisfiedCount { get; set; }

    public RequirementMode Mode { get; set; }

    public RequirementImportance Importance { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsScored { get; set; } = true;

    public int DisplayOrder { get; set; }
}
