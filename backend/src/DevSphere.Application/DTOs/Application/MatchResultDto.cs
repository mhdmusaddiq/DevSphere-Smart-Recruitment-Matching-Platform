namespace DevSphere.Application.DTOs.Application;

public class MatchResultDto
{
    public List<string> MatchedSkills { get; set; } = new();

    public List<string> MissingSkills { get; set; } = new();


    public double SkillsScore { get; set; }

    public double ExperienceScore { get; set; }

    public double EducationScore { get; set; }

    public double LocationScore { get; set; }

    public double TotalScore { get; set; }
}