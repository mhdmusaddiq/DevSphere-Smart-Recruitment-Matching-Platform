namespace DevSphere.Application.DTOs.Profile;

public class ProfileReadinessDto
{
    public bool IsReady { get; set; }

    public List<string> MissingItems { get; set; } = new();
}
