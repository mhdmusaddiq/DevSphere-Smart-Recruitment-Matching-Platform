using DevSphere.Application.DTOs.Files;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Services.Candidates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DevSphere.Tests.Files;

public class ResumeVersioningIntegrationTests
{
    [Fact]
    public async Task UploadV1_ThenV2_Snapshot_ThenV3_Preserves_All_Bytes_And_Frozen_Version()
    {
        await using var context = CreateContext();
        var storage = new TrackingStorage();
        var service = await CreateServiceAsync(context, storage);

        var v1Bytes = "%PDF v1"u8.ToArray();
        var v1 = await UploadAsync(service, v1Bytes, "v1.pdf");
        var v1Id = Assert.Single(v1!.Versions).Id;

        var v2Bytes = "%PDF v2"u8.ToArray();
        var v2 = await UploadAsync(service, v2Bytes, "v2.pdf");
        var v2Current = Assert.Single(v2!.Versions, x => x.IsCurrent);
        Assert.Equal(2, v2Current.VersionNumber);
        Assert.Equal(v2Current.Id, v2.CurrentVersionId);
        Assert.Contains(v2.Versions, x => x.Id == v1Id && !x.IsCurrent);

        var vacancyId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        context.Vacancies.Add(new Vacancy
        {
            Id = vacancyId,
            EmployerId = "employer-1",
            Title = "Engineer",
            LifecycleStatus = VacancyLifecycleStatus.Published,
            IsOpen = true
        });
        context.JobApplications.Add(new JobApplication
        {
            Id = applicationId,
            CandidateId = "candidate-1",
            VacancyId = vacancyId,
            Status = ApplicationStatus.Applied
        });
        context.ApplicationSnapshots.Add(new ApplicationSnapshot
        {
            Id = Guid.NewGuid(),
            JobApplicationId = applicationId,
            ResumeVersionId = v2Current.Id
        });
        await context.SaveChangesAsync();

        var v3Bytes = "%PDF v3"u8.ToArray();
        var v3 = await UploadAsync(service, v3Bytes, "v3.pdf");
        Assert.Equal(3, v3!.Versions.Count);
        Assert.Equal(3, Assert.Single(v3.Versions, x => x.IsCurrent).VersionNumber);

        await AssertStoredBytesAsync(service, v1Id, v1Bytes);
        await AssertStoredBytesAsync(service, v2Current.Id, v2Bytes);

        var frozen = await service.DownloadApplicationVersionAsync(
            "employer-1",
            applicationId);
        Assert.NotNull(frozen);
        using var frozenBytes = new MemoryStream();
        await frozen!.Content.CopyToAsync(frozenBytes);
        await frozen.Content.DisposeAsync();
        Assert.Equal(v2Bytes, frozenBytes.ToArray());
    }

    [Fact]
    public async Task Persistence_Failure_After_Blob_Write_Deletes_New_Blob_And_Leaves_Current_Version()
    {
        var interceptor = new FailNextSaveInterceptor();
        var databaseName = Guid.NewGuid().ToString("N");
        var options = new DbContextOptionsBuilder<DevSphereDbContext>()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(interceptor)
            .Options;

        var storage = new TrackingStorage();
        Guid v1Id;

        await using (var context = new DevSphereDbContext(options))
        {
            var service = await CreateServiceAsync(context, storage);
            var v1 = await UploadAsync(service, "%PDF v1"u8.ToArray(), "v1.pdf");
            v1Id = v1!.CurrentVersionId!.Value;

            interceptor.FailNextSave = true;

            await Assert.ThrowsAsync<InjectedPersistenceException>(
                () => UploadAsync(service, "%PDF v2"u8.ToArray(), "v2.pdf"));
        }

        Assert.Single(storage.Files);
        Assert.Single(storage.DeletedKeys);

        await using var verificationContext = new DevSphereDbContext(options);
        var persisted = await verificationContext.Resumes
            .Include(x => x.Versions)
            .SingleAsync();

        Assert.Equal(v1Id, persisted.CurrentVersionId);
        Assert.Single(persisted.Versions);
        Assert.True(persisted.Versions.Single().IsCurrent);
    }

    private static DevSphereDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DevSphereDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new DevSphereDbContext(options);
    }

    private static async Task<ResumeService> CreateServiceAsync(
        DevSphereDbContext context,
        IFileStorageService storage)
    {
        if (!await context.CandidateProfiles.AnyAsync())
        {
            context.CandidateProfiles.Add(new CandidateProfile
            {
                Id = Guid.NewGuid(),
                UserId = "candidate-1",
                FullName = "Candidate"
            });
            await context.SaveChangesAsync();
        }

        return new ResumeService(
            new ResumeRepository(context),
            new CandidateProfileRepository(context),
            storage,
            new JobApplicationRepository(context));
    }

    private static Task<DevSphere.Application.DTOs.Candidates.ResumeDto?> UploadAsync(
        ResumeService service,
        byte[] bytes,
        string fileName)
    {
        return service.AddUploadedVersionAsync(
            "candidate-1",
            new MemoryStream(bytes),
            fileName,
            "application/pdf",
            bytes.Length);
    }

    private static async Task AssertStoredBytesAsync(
        ResumeService service,
        Guid versionId,
        byte[] expected)
    {
        var download = await service.DownloadOwnVersionAsync(
            "candidate-1",
            versionId);
        Assert.NotNull(download);
        using var actual = new MemoryStream();
        await download!.Content.CopyToAsync(actual);
        await download.Content.DisposeAsync();
        Assert.Equal(expected, actual.ToArray());
    }

    private sealed class TrackingStorage : IFileStorageService
    {
        public Dictionary<string, byte[]> Files { get; } = new();
        public List<string> DeletedKeys { get; } = new();

        public async Task<StoredFileDescriptor> SaveAsync(
            Stream content,
            string fileName,
            string contentType,
            long fileSizeBytes,
            CancellationToken cancellationToken = default)
        {
            var key = $"{Guid.NewGuid():N}.pdf";
            using var copy = new MemoryStream();
            await content.CopyToAsync(copy, cancellationToken);
            Files[key] = copy.ToArray();
            return new StoredFileDescriptor
            {
                StorageKey = key,
                OriginalFileName = fileName,
                ContentType = contentType,
                FileSizeBytes = fileSizeBytes
            };
        }

        public Task<Stream> OpenReadAsync(
            string storageKey,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Stream>(new MemoryStream(Files[storageKey], writable: false));
        }

        public Task DeleteAsync(
            string storageKey,
            CancellationToken cancellationToken = default)
        {
            DeletedKeys.Add(storageKey);
            Files.Remove(storageKey);
            return Task.CompletedTask;
        }
    }

    private sealed class FailNextSaveInterceptor : SaveChangesInterceptor
    {
        public bool FailNextSave { get; set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (FailNextSave)
            {
                FailNextSave = false;
                throw new InjectedPersistenceException();
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    private sealed class InjectedPersistenceException : Exception;
}
