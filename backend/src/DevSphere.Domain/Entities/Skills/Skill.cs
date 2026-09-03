using DevSphere.Domain.Entities;

namespace DevSphere.Domain.Entities.Skills;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}