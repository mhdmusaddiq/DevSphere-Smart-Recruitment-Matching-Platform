using DevSphere.Domain.Entities.Skills;

namespace DevSphere.Domain.Entities.Candidates;

public class CandidateProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int ExperienceMonths { get; set; }

    public string Education { get; set; } = string.Empty;

    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
}