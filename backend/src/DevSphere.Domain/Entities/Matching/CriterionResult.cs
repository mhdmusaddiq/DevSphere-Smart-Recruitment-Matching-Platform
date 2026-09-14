namespace DevSphere.Domain.Entities.Matching;

public class CriterionResult : BaseEntity
{
    public Guid MatchResultId { get; set; }

    public string CriterionName { get; set; } = string.Empty;

    public double WeightPercent { get; set; }

    public double Score { get; set; }

    public string Details { get; set; } = string.Empty;

    public MatchResult MatchResult { get; set; } = null!;
}
