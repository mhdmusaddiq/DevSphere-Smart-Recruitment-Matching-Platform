namespace DevSphere.Application.DTOs.Profile;

public class CandidateProfileDto
{
    public string FullName { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int ExperienceMonths { get; set; }

    public string Education { get; set; } = string.Empty;
}