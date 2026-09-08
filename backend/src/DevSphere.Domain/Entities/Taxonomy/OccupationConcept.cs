using DevSphere.Domain.Entities;

namespace DevSphere.Domain.Entities.Taxonomy;

public class OccupationConcept : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
