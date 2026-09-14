namespace DevSphere.Domain.Entities.Career;

public class EducationRecord : BaseEntity
{
    public Guid CandidateProfileId { get; set; }

    public string Institution { get; set; } = string.Empty;

    public string Qualification { get; set; } = string.Empty;

    public string FieldOfStudy { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
