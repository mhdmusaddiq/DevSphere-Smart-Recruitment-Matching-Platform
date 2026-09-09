using DevSphere.Application.DTOs.Candidates;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Resume;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Candidates;

public class ResumeService : IResumeService
{
    private readonly ResumeRepository _repository;
    private readonly CandidateProfileRepository _profileRepository;
    private readonly IFileStorageService _storage;
    private readonly JobApplicationRepository _applicationRepository;

    public ResumeService(
        ResumeRepository repository,
        CandidateProfileRepository profileRepository,
        IFileStorageService storage,
        JobApplicationRepository applicationRepository)
    {
        _repository = repository;
        _profileRepository = profileRepository;
        _storage = storage;
        _applicationRepository = applicationRepository;
    }

    public async Task<ResumeDto?> GetOwnAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
        {
            return null;
        }

        var resume = await _repository.GetByCandidateProfileIdAsync(
            profile.Id,
            cancellationToken);

        return resume == null ? null : Map(resume);
    }

    public async Task<ResumeDto?> AddUploadedVersionAsync(
        string userId,
        Stream content,
        string fileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
        {
            return null;
        }

        var stored = await _storage.SaveAsync(
            content,
            fileName,
            contentType,
            fileSizeBytes,
            cancellationToken);

        var resume = await _repository.GetByCandidateProfileIdAsync(
            profile.Id,
            cancellationToken);

        var isNew = resume == null;

        resume ??= new Resume
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = profile.Id,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var existingVersion in resume.Versions)
        {
            existingVersion.IsCurrent = false;
        }

        var nextVersionNumber = resume.Versions.Count == 0
            ? 1
            : resume.Versions.Max(x => x.VersionNumber) + 1;

        var version = new ResumeVersion
        {
            Id = Guid.NewGuid(),
            ResumeId = resume.Id,
            VersionNumber = nextVersionNumber,
            OriginalFileName = stored.OriginalFileName,
            StorageKey = stored.StorageKey,
            ContentType = stored.ContentType,
            FileSizeBytes = stored.FileSizeBytes,
            IsCurrent = true,
            CreatedAt = DateTime.UtcNow
        };

        resume.Versions.Add(version);
        resume.CurrentVersionId = version.Id;
        resume.UpdatedAt = DateTime.UtcNow;

        if (isNew)
        {
            await _repository.AddAsync(
                resume,
                cancellationToken);
        }
        else
        {
            await _repository.SaveChangesAsync(
                cancellationToken);
        }

        return Map(resume);
    }


    public async Task<ResumeDto?> SetCurrentVersionAsync(
        string userId,
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository
            .GetByUserIdAsync(userId);

        if (profile == null)
        {
            return null;
        }

        var resume = await _repository
            .GetByCandidateProfileIdAsync(
                profile.Id,
                cancellationToken);

        if (resume == null)
        {
            return null;
        }

        var selectedVersion = resume.Versions
            .FirstOrDefault(x => x.Id == versionId);

        if (selectedVersion == null)
        {
            return null;
        }

        foreach (var version in resume.Versions)
        {
            version.IsCurrent =
                version.Id == selectedVersion.Id;
        }

        resume.CurrentVersionId = selectedVersion.Id;
        resume.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(
            cancellationToken);

        return Map(resume);
    }
    public async Task<ResumeDownloadDto?> DownloadOwnVersionAsync(
        string userId,
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
        {
            return null;
        }

        var resume = await _repository.GetByCandidateProfileIdAsync(
            profile.Id,
            cancellationToken);

        if (resume == null)
        {
            return null;
        }

        var version = resume.Versions
            .FirstOrDefault(x => x.Id == versionId);

        if (version == null)
        {
            return null;
        }

        var content = await _storage.OpenReadAsync(
            version.StorageKey,
            cancellationToken);

        return new ResumeDownloadDto
        {
            Content = content,
            FileName = version.OriginalFileName,
            ContentType = string.IsNullOrWhiteSpace(version.ContentType)
                ? "application/octet-stream"
                : version.ContentType
        };
    }

    public async Task<ResumeDownloadDto?> DownloadApplicationVersionAsync(
        string employerId,
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var application = await _applicationRepository
            .GetWithVacancyAsync(applicationId);

        if (application == null ||
            application.Vacancy.EmployerId != employerId ||
            application.Snapshot?.ResumeVersionId == null)
        {
            return null;
        }

        var version = await _repository.GetVersionAsync(
            application.Snapshot!.ResumeVersionId!.Value,
            cancellationToken);

        if (version == null)
        {
            return null;
        }

        return new ResumeDownloadDto
        {
            Content = await _storage.OpenReadAsync(
                version.StorageKey,
                cancellationToken),
            FileName = version.OriginalFileName,
            ContentType = "application/pdf"
        };
    }

    private static ResumeDto Map(Resume resume)
    {
        return new ResumeDto
        {
            Id = resume.Id,
            CandidateProfileId = resume.CandidateProfileId,
            CurrentVersionId = resume.CurrentVersionId,

            Versions = resume.Versions
                .OrderByDescending(x => x.VersionNumber)
                .Select(x => new ResumeVersionDto
                {
                    Id = x.Id,
                    VersionNumber = x.VersionNumber,
                    OriginalFileName = x.OriginalFileName,
                    StorageKey = x.StorageKey,
                    ContentType = x.ContentType,
                    FileSizeBytes = x.FileSizeBytes,
                    IsCurrent = x.IsCurrent
                })
                .ToList()
        };
    }
}
