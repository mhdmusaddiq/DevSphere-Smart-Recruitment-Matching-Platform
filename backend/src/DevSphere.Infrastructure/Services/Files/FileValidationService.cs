using DevSphere.Application.DTOs.Files;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;

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

        try
        {
            using var copy = new MemoryStream();
            await stream.CopyToAsync(copy, cancellationToken);
            var bytes = copy.ToArray();

            if (!HasCompletePdfEnvelope(bytes))
            {
                return Invalid("The uploaded file is not a valid PDF.");
            }

            using var document = PdfDocument.Open(bytes);

            // Force the catalogue and every page dictionary to be resolved.
            // PdfPig parses lazily, so opening alone is not a structural check.
            _ = document.Structure.Catalog;
            for (var pageNumber = 1; pageNumber <= document.NumberOfPages; pageNumber++)
            {
                _ = document.GetPage(pageNumber);
            }

            return new FileValidationResult { IsValid = true };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return Invalid("The uploaded file is not a valid PDF.");
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }

    public async Task<MemoryStream> BufferAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        if (!stream.CanRead)
        {
            throw new InvalidDataException("The uploaded file could not be read.");
        }

        var buffer = new MemoryStream();

        try
        {
            var chunk = new byte[81920];
            long total = 0;

            while (true)
            {
                var bytesRead = await stream.ReadAsync(chunk, cancellationToken);
                if (bytesRead == 0)
                {
                    break;
                }

                total += bytesRead;
                if (total > _options.MaximumFileSizeBytes)
                {
                    throw new InvalidDataException("The PDF exceeds the 5 MiB size limit.");
                }

                await buffer.WriteAsync(chunk.AsMemory(0, bytesRead), cancellationToken);
            }

            buffer.Position = 0;
            return buffer;
        }
        catch
        {
            await buffer.DisposeAsync();
            throw;
        }
    }

    private static bool HasCompletePdfEnvelope(byte[] bytes)
    {
        if (bytes.Length < 16)
        {
            return false;
        }

        var headerSearchLength = Math.Min(bytes.Length, 1024);
        var headerIndex = bytes.AsSpan(0, headerSearchLength).IndexOf("%PDF-"u8);
        if (headerIndex < 0)
        {
            return false;
        }

        var eofIndex = bytes.AsSpan().LastIndexOf("%%EOF"u8);
        if (eofIndex < 0)
        {
            return false;
        }

        var startXrefIndex = bytes.AsSpan(0, eofIndex).LastIndexOf("startxref"u8);
        return startXrefIndex >= 0;
    }

    private static FileValidationResult Invalid(string error)
    {
        return new FileValidationResult { IsValid = false, Error = error };
    }
}
