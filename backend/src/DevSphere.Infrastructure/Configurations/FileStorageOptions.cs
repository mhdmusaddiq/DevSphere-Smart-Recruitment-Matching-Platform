namespace DevSphere.Infrastructure.Configurations;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = "App_Data/Files";

    public long MaximumFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = [".pdf"];

    public string[] AllowedContentTypes { get; set; } =
    [
        "application/pdf"
    ];
}
