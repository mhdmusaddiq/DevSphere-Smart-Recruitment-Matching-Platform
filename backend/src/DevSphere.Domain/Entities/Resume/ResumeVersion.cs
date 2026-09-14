namespace DevSphere.Domain.Entities.Resume;

public class ResumeVersion : BaseEntity
{
    public Guid ResumeId { get; set; }

    public int VersionNumber { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public bool IsCurrent { get; set; }

    public Resume Resume { get; set; } = null!;
}
