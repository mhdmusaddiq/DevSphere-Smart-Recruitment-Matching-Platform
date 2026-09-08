namespace DevSphere.Application.DTOs.Profile;

public class ApplicationReadinessDto
{
    public bool IsReady { get; set; }

    public bool ProfileReady { get; set; }

    public bool ResumeReady { get; set; }

    public Guid? CurrentResumeVersionId { get; set; }

    public List<string> MissingItems { get; set; } = new();
}
