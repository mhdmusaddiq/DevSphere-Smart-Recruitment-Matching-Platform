using DevSphere.Application.DTOs.Files;

namespace DevSphere.Application.Interfaces;

public interface IFileValidationService
{
    FileValidationResult Validate(string fileName, string contentType, long fileSizeBytes);

    Task<MemoryStream> BufferAsync(Stream stream, CancellationToken cancellationToken);

    Task<FileValidationResult> ValidatePdfContentAsync(Stream stream, CancellationToken cancellationToken);
}
