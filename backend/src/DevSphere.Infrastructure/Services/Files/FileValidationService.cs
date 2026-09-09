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

    private static FileValidationResult Invalid(string error)
    {
        return new FileValidationResult { IsValid = false, Error = error };
    }
}
