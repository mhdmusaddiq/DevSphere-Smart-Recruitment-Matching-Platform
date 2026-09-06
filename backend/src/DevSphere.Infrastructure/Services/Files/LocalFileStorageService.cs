using DevSphere.Application.DTOs.Files;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace DevSphere.Infrastructure.Services.Files;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IOptions<FileStorageOptions> options)
    {
        _rootPath = Path.GetFullPath(options.Value.RootPath, AppContext.BaseDirectory);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<StoredFileDescriptor> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var storageKey = $"{Guid.NewGuid():N}{extension}";
        var fullPath = ResolvePath(storageKey);

        await using var destination = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        await content.CopyToAsync(destination, cancellationToken);

        return new StoredFileDescriptor
        {
            StorageKey = storageKey,
            OriginalFileName = Path.GetFileName(fileName),
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes
        };
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(
            ResolvePath(storageKey),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    private string ResolvePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || Path.GetFileName(storageKey) != storageKey)
        {
            throw new ArgumentException("Invalid storage key.", nameof(storageKey));
        }

        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, storageKey));
        var rootWithSeparator = _rootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The storage key resolves outside the configured storage root.");
        }

        return fullPath;
    }
}
