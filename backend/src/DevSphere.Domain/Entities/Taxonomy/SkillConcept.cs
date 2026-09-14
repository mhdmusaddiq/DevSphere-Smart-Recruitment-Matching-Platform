using DevSphere.Domain.Entities;

namespace DevSphere.Domain.Entities.Taxonomy;

public class SkillConcept : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<SkillAlias> Aliases { get; set; } =
        new List<SkillAlias>();
}
