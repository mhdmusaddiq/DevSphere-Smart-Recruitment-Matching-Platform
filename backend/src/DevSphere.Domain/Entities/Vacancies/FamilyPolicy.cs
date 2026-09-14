using DevSphere.Domain.Enums;

namespace DevSphere.Domain.Entities.Vacancies;

public class FamilyPolicy : BaseEntity
{
    public Guid MatchingPolicyRevisionId { get; set; }

    public RequirementFamily RequirementFamily { get; set; }

    public RequirementImportance FamilyImportance { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsScored { get; set; } = true;
}
