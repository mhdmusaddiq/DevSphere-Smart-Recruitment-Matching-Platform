namespace DevSphere.Infrastructure.Configurations;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = "App_Data/Files";

    public long MaximumFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = [".pdf", ".doc", ".docx"];

    public string[] AllowedContentTypes { get; set; } =
    [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];
}
