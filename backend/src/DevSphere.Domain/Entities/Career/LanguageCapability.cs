namespace DevSphere.Domain.Entities.Career;

public class LanguageCapability : BaseEntity
{
    public Guid CandidateProfileId { get; set; }

    public string Language { get; set; } = string.Empty;

    public string Proficiency { get; set; } = string.Empty;
}
