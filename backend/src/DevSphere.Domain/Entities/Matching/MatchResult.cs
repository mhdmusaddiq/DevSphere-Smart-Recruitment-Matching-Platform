namespace DevSphere.Domain.Entities.Matching;

public class MatchResult : BaseEntity
{
    public Guid CandidateProfileId { get; set; }

    public Guid VacancyId { get; set; }

    public string MatchedSkills { get; set; } = string.Empty;

    public string MissingSkills { get; set; } = string.Empty;

    public double FinalScore { get; set; }

    public DateTime CalculatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<CriterionResult> CriterionResults { get; set; } = new List<CriterionResult>();
}
