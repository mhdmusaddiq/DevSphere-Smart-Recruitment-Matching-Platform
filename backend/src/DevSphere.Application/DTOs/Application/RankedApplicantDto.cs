namespace DevSphere.Application.DTOs.Application;

public class RankedApplicantDto
{
    public string CandidateId { get; set; } = string.Empty;

    public Guid VacancyId { get; set; }

    public string Status { get; set; } = string.Empty;

    public double MatchScore { get; set; }

    public List<string> MatchedSkills { get; set; } = new();

    public List<string> MissingSkills { get; set; } = new();
}
