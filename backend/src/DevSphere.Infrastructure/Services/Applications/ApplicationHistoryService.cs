using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Applications;

public class ApplicationHistoryService : IApplicationHistoryService
{
    private readonly ApplicationHistoryRepository _repository;

    public ApplicationHistoryService(ApplicationHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationSnapshotDto> CreateSnapshotAsync(ApplicationSnapshotDto request, CancellationToken cancellationToken = default)
    {
        var entity = new ApplicationSnapshot
        {
            Id = Guid.NewGuid(),
            JobApplicationId = request.JobApplicationId,
            ResumeVersionId = request.ResumeVersionId,
            MatchingPolicyRevisionId = request.MatchingPolicyRevisionId,
            CompatibilityScore = request.CompatibilityScore,
            RawCompatibilityScore = request.RawCompatibilityScore,
            DisplayCompatibilityScore = request.DisplayCompatibilityScore,
            HighTierAggregateScore = request.HighTierAggregateScore,
            MediumTierAggregateScore = request.MediumTierAggregateScore,
            Coverage = request.Coverage,
            CompatibilityStatus = request.CompatibilityStatus,
            EligibilityStatus = request.EligibilityStatus,
            EligibilityReason = request.EligibilityReason,
            IsEligible = request.IsEligible,
            ApplyDecision = request.ApplyDecision,
            MatchedSkillsJson = request.MatchedSkillsJson,
            GapSkillsJson = request.GapSkillsJson,
            EvidenceSummaryJson = request.EvidenceSummaryJson,
            MatchResultJson = request.MatchResultJson,
            CandidateSnapshotJson = request.CandidateSnapshotJson,
            VacancySnapshotJson = request.VacancySnapshotJson,
            CapturedAtUtc = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddSnapshotAsync(entity, cancellationToken);
        request.Id = entity.Id;
        request.CapturedAtUtc = entity.CapturedAtUtc;
        return request;
    }

    public async Task<ApplicationSnapshotDto?> GetSnapshotAsync(
        Guid jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await _repository.GetSnapshotAsync(
                jobApplicationId,
                cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return new ApplicationSnapshotDto
        {
            Id = entity.Id,
            JobApplicationId = entity.JobApplicationId,
            ResumeVersionId = entity.ResumeVersionId,
            MatchingPolicyRevisionId = entity.MatchingPolicyRevisionId,
            CompatibilityScore = entity.CompatibilityScore,
            RawCompatibilityScore = entity.RawCompatibilityScore,
            DisplayCompatibilityScore = entity.DisplayCompatibilityScore,
            HighTierAggregateScore = entity.HighTierAggregateScore,
            MediumTierAggregateScore = entity.MediumTierAggregateScore,
            Coverage = entity.Coverage,
            CompatibilityStatus = entity.CompatibilityStatus,
            EligibilityStatus = entity.EligibilityStatus,
            EligibilityReason = entity.EligibilityReason,
            IsEligible = entity.IsEligible,
            ApplyDecision = entity.ApplyDecision,
            MatchedSkillsJson = entity.MatchedSkillsJson,
            GapSkillsJson = entity.GapSkillsJson,
            EvidenceSummaryJson = entity.EvidenceSummaryJson,
            MatchResultJson = entity.MatchResultJson,
            CandidateSnapshotJson = entity.CandidateSnapshotJson,
            VacancySnapshotJson = entity.VacancySnapshotJson,
            CapturedAtUtc = entity.CapturedAtUtc
        };
    }
    public async Task<ApplicationStatusHistoryDto> RecordStatusAsync(ApplicationStatusHistoryDto request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ApplicationStatus>(request.NewStatus, true, out var newStatus))
        {
            throw new ArgumentException("Invalid application status.", nameof(request));
        }

        ApplicationStatus? previousStatus = null;
        if (!string.IsNullOrWhiteSpace(request.PreviousStatus) &&
            Enum.TryParse<ApplicationStatus>(request.PreviousStatus, true, out var parsedPreviousStatus))
        {
            previousStatus = parsedPreviousStatus;
        }

        var entity = new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            JobApplicationId = request.JobApplicationId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ChangedByUserId = request.ChangedByUserId,
            ChangedAtUtc = DateTime.UtcNow,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddStatusAsync(entity, cancellationToken);
        request.Id = entity.Id;
        request.ChangedAtUtc = entity.ChangedAtUtc;
        return request;
    }

    public async Task<IReadOnlyList<ApplicationStatusHistoryDto>> GetStatusHistoryAsync(Guid jobApplicationId, CancellationToken cancellationToken = default)
    {
        var history = await _repository.GetStatusHistoryAsync(jobApplicationId, cancellationToken);
        return history.Select(x => new ApplicationStatusHistoryDto
        {
            Id = x.Id,
            JobApplicationId = x.JobApplicationId,
            PreviousStatus = x.PreviousStatus?.ToString(),
            NewStatus = x.NewStatus.ToString(),
            ChangedByUserId = x.ChangedByUserId,
            ChangedAtUtc = x.ChangedAtUtc,
            Notes = x.Notes
        }).ToList();
    }
}
