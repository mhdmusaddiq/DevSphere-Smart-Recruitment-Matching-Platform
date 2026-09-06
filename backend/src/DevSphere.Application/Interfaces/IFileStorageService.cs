using DevSphere.Application.DTOs.Files;

namespace DevSphere.Application.Interfaces;

public interface IFileStorageService
{
    Task<StoredFileDescriptor> SaveAsync(Stream content, string fileName, string contentType, long fileSizeBytes, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
}
