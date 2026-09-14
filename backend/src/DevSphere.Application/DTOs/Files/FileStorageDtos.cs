namespace DevSphere.Application.DTOs.Files;

public class StoredFileDescriptor
{
    public string StorageKey { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }
}

public class FileValidationResult
{
    public bool IsValid { get; set; }

    public string? Error { get; set; }

    public bool TooLarge { get; set; }
}
