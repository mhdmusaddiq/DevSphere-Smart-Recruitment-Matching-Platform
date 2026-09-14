using DevSphere.Domain.Entities;
using DevSphere.Domain.Entities.Taxonomy;

namespace DevSphere.Domain.Entities.Skills;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // Candidate skills resolve to one canonical concept.
    // Nullable preserves compatibility for existing vacancy/free-text rows
    // until the coordinator upgrades the shared matching model.
    public Guid? SkillConceptId { get; set; }

    public SkillConcept? SkillConcept { get; set; }
}
