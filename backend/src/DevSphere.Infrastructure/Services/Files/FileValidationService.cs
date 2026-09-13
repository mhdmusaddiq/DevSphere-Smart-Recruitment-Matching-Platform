using DevSphere.Application.DTOs.Files;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace DevSphere.Infrastructure.Services.Files;

public class FileValidationService : IFileValidationService
{
    private readonly FileStorageOptions _options;

    public FileValidationService(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;
    }

    public FileValidationResult Validate(string fileName, string contentType, long fileSizeBytes)
    {
        if (fileSizeBytes <= 0 || fileSizeBytes > _options.MaximumFileSizeBytes)
        {
            return new FileValidationResult
            {
                IsValid = false,
                TooLarge = fileSizeBytes > _options.MaximumFileSizeBytes,
                Error = "The PDF is empty or exceeds the 5 MiB size limit."
            };
        }

        var extension = Path.GetExtension(fileName);
        if (!_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return Invalid("The file extension is not allowed.");
        }

        if (!_options.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            return Invalid("The file content type is not allowed.");
        }

        return new FileValidationResult { IsValid = true };
    }

    public async Task<FileValidationResult> ValidatePdfContentAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        if (!stream.CanRead)
        {
            return Invalid("The uploaded file could not be read.");
        }

        var originalPosition = stream.CanSeek ? stream.Position : 0;
        var signature = new byte[5];
        var bytesRead = await stream.ReadAsync(signature, cancellationToken);

        if (stream.CanSeek)
        {
            stream.Position = originalPosition;
        }

        return bytesRead == signature.Length && signature.SequenceEqual("%PDF-"u8.ToArray())
            ? new FileValidationResult { IsValid = true }
            : Invalid("The uploaded file is not a valid PDF.");
    }

    private static FileValidationResult Invalid(string error)
    {
        return new FileValidationResult { IsValid = false, Error = error };
    }
}
