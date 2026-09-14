namespace DevSphere.Application.DTOs.Candidates;

public class ResumeDto
{
    public Guid Id { get; set; }

    public Guid CandidateProfileId { get; set; }

    public Guid? CurrentVersionId { get; set; }

    public List<ResumeVersionDto> Versions { get; set; } = new();
}

public class ResumeVersionDto
{
    public Guid Id { get; set; }

    public int VersionNumber { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public bool IsCurrent { get; set; }
}
