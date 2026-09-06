using DevSphere.Application.DTOs.Candidates;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Resume;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Candidates;

public class ResumeService : IResumeService
{
    private readonly ResumeRepository _repository;

    public ResumeService(ResumeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResumeDto?> GetAsync(Guid candidateProfileId, CancellationToken cancellationToken = default)
    {
        var resume = await _repository.GetByCandidateProfileIdAsync(candidateProfileId, cancellationToken);
        return resume == null ? null : Map(resume);
    }

    public async Task<ResumeDto> AddVersionAsync(Guid candidateProfileId, ResumeVersionDto request, CancellationToken cancellationToken = default)
    {
        var resume = await _repository.GetByCandidateProfileIdAsync(candidateProfileId, cancellationToken);
        var isNew = resume == null;

        resume ??= new Resume
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = candidateProfileId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var existingVersion in resume.Versions)
        {
            existingVersion.IsCurrent = false;
        }

        var version = new ResumeVersion
        {
            Id = Guid.NewGuid(),
            ResumeId = resume.Id,
            VersionNumber = resume.Versions.Count + 1,
            OriginalFileName = request.OriginalFileName,
            StorageKey = request.StorageKey,
            ContentType = request.ContentType,
            FileSizeBytes = request.FileSizeBytes,
            IsCurrent = true,
            CreatedAt = DateTime.UtcNow
        };

        resume.Versions.Add(version);
        resume.CurrentVersionId = version.Id;
        resume.UpdatedAt = DateTime.UtcNow;

        if (isNew)
        {
            await _repository.AddAsync(resume, cancellationToken);
        }
        else
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return Map(resume);
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
