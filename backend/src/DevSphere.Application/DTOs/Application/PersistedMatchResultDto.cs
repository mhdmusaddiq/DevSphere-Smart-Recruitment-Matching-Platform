namespace DevSphere.Application.DTOs.Application;

public class PersistedMatchResultDto
{
    public Guid Id { get; set; }

    public Guid CandidateProfileId { get; set; }

    public Guid VacancyId { get; set; }

    public string MatchedSkills { get; set; } = string.Empty;

    public string MissingSkills { get; set; } = string.Empty;

    public double FinalScore { get; set; }

    public DateTime CalculatedAtUtc { get; set; }
}
