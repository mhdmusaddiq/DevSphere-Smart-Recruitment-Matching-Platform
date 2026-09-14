using DevSphere.Domain.Entities;

namespace DevSphere.Domain.Entities.Taxonomy;

public class SkillAlias : BaseEntity
{
    public Guid SkillConceptId { get; set; }

    public string Alias { get; set; } = string.Empty;

    public string NormalizedAlias { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public SkillConcept SkillConcept { get; set; } = null!;
}
