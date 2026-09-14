namespace DevSphere.Domain.Entities.Resume;

public class Resume : BaseEntity
{
    public Guid CandidateProfileId { get; set; }

    public Guid? CurrentVersionId { get; set; }

    public ICollection<ResumeVersion> Versions { get; set; } = new List<ResumeVersion>();
}
