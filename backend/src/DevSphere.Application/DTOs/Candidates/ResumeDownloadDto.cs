namespace DevSphere.Application.DTOs.Candidates;

public class ResumeDownloadDto
{
    public Stream Content { get; set; } = Stream.Null;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/octet-stream";
}
