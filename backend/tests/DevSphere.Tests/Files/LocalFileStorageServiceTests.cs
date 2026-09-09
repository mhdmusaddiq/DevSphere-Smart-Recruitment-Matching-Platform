using System.Text;
using DevSphere.Infrastructure.Configurations;
using DevSphere.Infrastructure.Services.Files;
using Microsoft.Extensions.Options;

namespace DevSphere.Tests.Files;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _rootPath;
    private readonly LocalFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        _rootPath = Path.Combine(
            Path.GetTempPath(),
            "devsphere-tests",
            Guid.NewGuid().ToString("N"));

        var options = Options.Create(new FileStorageOptions
        {
            RootPath = _rootPath
        });

        _service = new LocalFileStorageService(options);
    }

    [Fact]
    public async Task SaveAsync_Should_Generate_Safe_Storage_Key()
    {
        var bytes = Encoding.UTF8.GetBytes("test cv");

        await using var content =
            new MemoryStream(bytes);

        var stored = await _service.SaveAsync(
            content,
            "../../candidate-cv.pdf",
            "application/pdf",
            bytes.Length);

        Assert.False(
            string.IsNullOrWhiteSpace(stored.StorageKey));

        Assert.Equal(
            stored.StorageKey,
            Path.GetFileName(stored.StorageKey));

        Assert.EndsWith(
            ".pdf",
            stored.StorageKey,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            "candidate-cv.pdf",
            stored.OriginalFileName);
    }

    [Fact]
    public async Task OpenReadAsync_Should_Reject_Path_Traversal()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            async () =>
            {
                await _service.OpenReadAsync(
                    "../secret.pdf");
            });
    }

    [Fact]
    public async Task Save_Then_Open_Should_Return_Stored_Content()
    {
        const string expected = "candidate resume content";

        var bytes = Encoding.UTF8.GetBytes(expected);

        await using var content =
            new MemoryStream(bytes);

        var stored = await _service.SaveAsync(
            content,
            "candidate.pdf",
            "application/pdf",
            bytes.Length);

        await using var opened =
            await _service.OpenReadAsync(
                stored.StorageKey);

        using var reader =
            new StreamReader(opened);

        var actual =
            await reader.ReadToEndAsync();

        Assert.Equal(expected, actual);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(
                _rootPath,
                recursive: true);
        }
    }
}
