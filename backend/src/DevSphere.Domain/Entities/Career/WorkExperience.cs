namespace DevSphere.Domain.Entities.Career;

public class WorkExperience : BaseEntity
{
    public Guid CandidateProfileId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Description { get; set; } = string.Empty;
}
