namespace DevSphere.Domain.Entities.Career;

public class ProjectRecord : BaseEntity
{
    public Guid CandidateProfileId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ProjectUrl { get; set; } = string.Empty;
}
