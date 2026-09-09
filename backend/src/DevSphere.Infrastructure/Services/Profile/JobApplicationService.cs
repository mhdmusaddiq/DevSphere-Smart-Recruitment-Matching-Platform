using Microsoft.EntityFrameworkCore;
using DevSphere.Application.Exceptions;
using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Profile;

public class JobApplicationService : IJobApplicationService
{
    private readonly JobApplicationRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly IMatchEngine _matchEngine;
    private readonly IApplicationHistoryService _applicationHistoryService;
    private readonly ICandidateProfileService _candidateProfileService;
    private readonly CandidateProfileRepository _candidateProfileRepository;
    private readonly ResumeRepository _resumeRepository;
    private readonly VacancyRepository _vacancyRepository;
    private readonly DevSphere.Infrastructure.Data.DevSphereDbContext _context;

    public JobApplicationService(
        JobApplicationRepository repository,
        INotificationService notificationService,
        IMatchEngine matchEngine,
        IApplicationHistoryService applicationHistoryService,
        ICandidateProfileService candidateProfileService,
        CandidateProfileRepository candidateProfileRepository,
        ResumeRepository resumeRepository,
        VacancyRepository vacancyRepository,
        DevSphere.Infrastructure.Data.DevSphereDbContext context)
    {
        _repository = repository;
        _notificationService = notificationService;
        _matchEngine = matchEngine;
        _applicationHistoryService = applicationHistoryService;
        _candidateProfileService = candidateProfileService;
        _candidateProfileRepository = candidateProfileRepository;
        _resumeRepository = resumeRepository;
        _vacancyRepository = vacancyRepository;
        _context = context;
    }


    public async Task<JobApplicationDto> ApplyAsync(
        string candidateId,
        JobApplicationDto request)
    {
        if (request.VacancyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Vacancy is required.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(candidateId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated candidate is required.");
        }

        var accountIsActive = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == candidateId)
            .Select(x => (bool?)x.IsActive)
            .FirstOrDefaultAsync();

        var accountStateBlocked =
            !accountIsActive.HasValue ||
            !accountIsActive.Value;

        var exists = await _repository.ExistsAsync(
            candidateId,
            request.VacancyId);

        if (exists)
        {
            throw new ApplicationConflictException(
                "Application already exists.");
        }

        var candidate =
            await _candidateProfileRepository
                .GetByUserIdWithSkillsAsync(
                    candidateId);

        if (candidate == null)
        {
            throw new InvalidOperationException(
                "Candidate profile is required before applying.");
        }

        var vacancy =
            await _vacancyRepository
                .GetByIdAsync(request.VacancyId);

        if (vacancy == null)
        {
            throw new KeyNotFoundException(
                "Vacancy not found.");
        }


        var resume =
            await _resumeRepository
                .GetByCandidateProfileIdAsync(
                    candidate.Id);

        var currentResume =
            resume?.Versions
                .Where(x => x.IsCurrent)
                .OrderByDescending(x => x.VersionNumber)
                .ThenByDescending(x => x.CreatedAt)
                .FirstOrDefault();

        var selectedResumeVersion =
            resume != null &&
            currentResume != null &&
            resume.CurrentVersionId == currentResume.Id
                ? currentResume
                : null;

        DevSphere.Application.DTOs.Application.MatchResultDto? match = null;
        var calculationFailure = false;

        try
        {
            match =
                await _matchEngine.CalculateAsync(
                    candidateId,
                    request.VacancyId.ToString());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            calculationFailure = true;
        }

        var currentPolicy =
            await _context.MatchingPolicyRevisions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.VacancyId == request.VacancyId &&
                    x.IsCurrent);
        var readiness =
            await _candidateProfileService
                .GetApplicationReadinessAsync(candidateId);

        var readinessMissingItems =
            readiness.MissingItems.ToList();

        var blockedByReadiness =
            !readiness.IsReady;

        // The selected CV must still be the canonical current version
        // immediately before the final apply decision is persisted.
        if (selectedResumeVersion != null &&
            readiness.CurrentResumeVersionId.HasValue &&
            readiness.CurrentResumeVersionId.Value !=
                selectedResumeVersion.Id)
        {
            blockedByReadiness = true;

            if (!readinessMissingItems.Contains(
                    "Selected resume version is stale."))
            {
                readinessMissingItems.Add(
                    "Selected resume version is stale.");
            }
        }

        if (selectedResumeVersion == null)
        {
            blockedByReadiness = true;

            if (!readinessMissingItems.Contains(
                    "Current resume version"))
            {
                readinessMissingItems.Add(
                    "Current resume version");
            }
        }

        var regulatoryRequirements =
            currentPolicy == null
                ? new List<DevSphere.Domain.Entities.Vacancies.VacancyRequirement>()
                : await _context.VacancyRequirements
                    .AsNoTracking()
                    .Where(x =>
                        x.MatchingPolicyRevisionId ==
                            currentPolicy.Id &&
                        x.IsActive &&
                        x.IsRegulatoryGate)
                    .ToListAsync();

        // Current runtime exposes regulatory metadata but does not yet
        // expose the RM-2.1 criterion evaluation result. Do not invent
        // a second evaluator here. The reducer supports the state when
        // an authoritative regulatory result becomes available.
        var blockedByRegulatoryGate = false;

        // No relationship-block aggregate exists on this frozen base.
        var relationshipBlocked = false;

        // No baseline-acknowledgement aggregate exists on this frozen base.
        var baselineAcknowledgedRequired = false;

        var vacancyUnavailableOrSuppressed =
            vacancy.LifecycleStatus !=
                DevSphere.Domain.Enums.VacancyLifecycleStatus.Published ||
            !vacancy.IsOpen ||
            (vacancy.ClosingDateUtc.HasValue &&
             vacancy.ClosingDateUtc.Value <= DateTime.UtcNow);

        var decision = ResolveApplyDecision(
            calculationFailure,
            accountStateBlocked,
            vacancyUnavailableOrSuppressed,
            relationshipBlocked,
            blockedByRegulatoryGate,
            blockedByReadiness,
            baselineAcknowledgedRequired);

        var isEligible =
            decision == "Allowed" ||
            decision ==
                "AllowedAfterBaselineAcknowledgement";

        var rawCompatibilityScore =
            Convert.ToDecimal(match?.TotalScore ?? 0);

        var displayCompatibilityScore =
            Math.Round(
                rawCompatibilityScore,
                2,
                MidpointRounding.AwayFromZero);

        var compatibilityStatus = "Calculated";

        var eligibilityStatus = isEligible ? "Eligible" : decision;

        var evidenceSummary = new
        {
            matchedSkills = match?.MatchedSkills ?? new List<string>(),
            missingSkills = match?.MissingSkills ?? new List<string>(),
            candidateExperienceMonths =
                candidate.ExperienceMonths,
            requiredExperienceMonths =
                vacancy.MinExperienceMonths,
            resumeVersionId = selectedResumeVersion?.Id,
            readinessMissingItems,
            regulatoryRequirementIds =
                regulatoryRequirements.Select(x => x.Id),
            compatibilityStatus,
            eligibilityStatus,
            applyDecision = decision
        };
        if (!isEligible)
        {
            throw new InvalidOperationException(
                $"Application blocked: {decision}.");
        }

        var now = DateTime.UtcNow;

        var application =
            new DevSphere.Domain.Entities.Applications.JobApplication
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                VacancyId = request.VacancyId,
                Status =
                    DevSphere.Domain.Enums.ApplicationStatus.Applied,
                AppliedAt = now,
                CreatedAt = now
            };

        var snapshot =
            new DevSphere.Domain.Entities.Applications.ApplicationSnapshot
            {
                Id = Guid.NewGuid(),
                JobApplicationId = application.Id,
                ResumeVersionId = selectedResumeVersion?.Id,
                MatchingPolicyRevisionId =
                    currentPolicy?.Id,
                CompatibilityScore = displayCompatibilityScore,
                RawCompatibilityScore = rawCompatibilityScore,
                DisplayCompatibilityScore =
                    displayCompatibilityScore,
                CompatibilityStatus = compatibilityStatus,
                EligibilityStatus = eligibilityStatus,
                IsEligible = isEligible,
                ApplyDecision = decision,
                CandidateSnapshotJson =
                    System.Text.Json.JsonSerializer.Serialize(
                        new
                        {
                            candidate.Id,
                            candidate.UserId,
                            candidate.FullName,
                            candidate.Location,
                            candidate.ExperienceMonths,
                            candidate.Education,
                            candidate.PreferredWorkMode,
                            candidate.PreferredLocation,
                            candidate.WillingToRelocate,
                            candidate.PreferredEmploymentType,
                            candidate.AvailabilityStatus,
                            candidate.AvailableFrom,
                            candidate.NoticePeriodDays,
                            skills = candidate.Skills
                                .OrderBy(x => x.Name)
                                .Select(x => new
                                {
                                    x.Id,
                                    x.Name,
                                    x.SkillConceptId
                                })
                        }),
                VacancySnapshotJson =
                    System.Text.Json.JsonSerializer.Serialize(
                        new
                        {
                            vacancy.Id,
                            vacancy.Title,
                            vacancy.Description,
                            vacancy.Location,
                            vacancy.MinExperienceMonths,
                            vacancy.MaxExperienceMonths,
                            vacancy.RequiredEducation,
                            vacancy.SalaryMin,
                            vacancy.SalaryMax,
                            vacancy.ClosingDateUtc,
                            vacancy.LifecycleStatus,
                            vacancy.IsOpen,
                            matchingPolicyRevisionId =
                                currentPolicy?.Id,
                            matchingPolicyRevisionNumber =
                                currentPolicy?.RevisionNumber
                        }),
                MatchedSkillsJson =
                    System.Text.Json.JsonSerializer.Serialize(
                        match?.MatchedSkills ?? new List<string>()),
                GapSkillsJson =
                    System.Text.Json.JsonSerializer.Serialize(
                        match?.MissingSkills ?? new List<string>()),
                EvidenceSummaryJson =
                    System.Text.Json.JsonSerializer.Serialize(
                        evidenceSummary),
                CapturedAtUtc = now,
                CreatedAt = now
            };

        await _repository.AddWithSnapshotAsync(
            application,
            snapshot);

        return new JobApplicationDto
        {
            Id = application.Id,
            CandidateId = application.CandidateId,
            VacancyId = application.VacancyId,
            Status = application.Status.ToString()
        };
    }

    public async Task<List<JobApplicationDto>> GetByCandidateAsync(
        string candidateId)
    {
        var applications = await _repository
            .GetByCandidateAsync(candidateId);


        return applications
    .Select(x => new JobApplicationDto
    {
        Id = x.Id,
        CandidateId = x.CandidateId,
        VacancyId = x.VacancyId,
        Status = x.Status.ToString()
    })
    .ToList();
    }

    public async Task<List<RankedApplicantDto>> GetByVacancyAsync(
        Guid vacancyId,
        string employerId)
    {
        var applications = await _repository
            .GetByVacancyAsync(vacancyId, employerId);

        var rankedApplicants = new List<RankedApplicantDto>();

        foreach (var application in applications)
        {
            var match = await _matchEngine.CalculateAsync(
                application.CandidateId,
                vacancyId.ToString());

            rankedApplicants.Add(new RankedApplicantDto
            {
                CandidateId = application.CandidateId,
                VacancyId = application.VacancyId,
                Status = application.Status.ToString(),
                MatchScore = match.TotalScore,
                MatchedSkills = match.MatchedSkills,
                MissingSkills = match.MissingSkills
            });
        }

        return rankedApplicants
            .OrderByDescending(x => x.MatchScore)
            .ThenBy(
                x => x.CandidateId,
                StringComparer.Ordinal)
            .ToList();
    }



    public async Task<JobApplicationDto> UpdateStatusAsync(
        Guid applicationId,
        string status,
        string changedByUserId)
    {
        if (string.IsNullOrWhiteSpace(changedByUserId))
        {
            throw new ArgumentException(
                "Changed-by user is required.",
                nameof(changedByUserId));
        }

        var application = await _repository
            .GetByIdAsync(applicationId);

        if (application == null)
        {
            throw new KeyNotFoundException(
                "Application not found.");
        }

        if (!Enum.TryParse<
                DevSphere.Domain.Enums.ApplicationStatus>(
                status,
                true,
                out var newStatus) ||
            !Enum.IsDefined(newStatus))
        {
            throw new ArgumentException(
                "Invalid application status.",
                nameof(status));
        }

        var previousStatus = application.Status;

        if (!IsValidTransition(previousStatus, newStatus))
        {
            throw new InvalidOperationException(
                $"Invalid application status transition from {previousStatus} to {newStatus}.");
        }

        application.Status = newStatus;
        application.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(application);

        await _applicationHistoryService.RecordStatusAsync(
            new ApplicationStatusHistoryDto
            {
                JobApplicationId = application.Id,
                PreviousStatus = previousStatus.ToString(),
                NewStatus = newStatus.ToString(),
                ChangedByUserId = changedByUserId,
                Notes = $"Status changed from {previousStatus} to {newStatus}."
            });

        await _notificationService.CreateAsync(
            application.CandidateId,
            $"Your application status has been updated to {newStatus}.");

        return new JobApplicationDto
        {
            Id = application.Id,
            CandidateId = application.CandidateId,
            VacancyId = application.VacancyId,
            Status = application.Status.ToString()
        };
    }

    public static bool IsValidTransition(
        DevSphere.Domain.Enums.ApplicationStatus current,
        DevSphere.Domain.Enums.ApplicationStatus next)
    {
        return current switch
        {
            DevSphere.Domain.Enums.ApplicationStatus.Applied =>
                next is DevSphere.Domain.Enums.ApplicationStatus.UnderReview or DevSphere.Domain.Enums.ApplicationStatus.Rejected,

            DevSphere.Domain.Enums.ApplicationStatus.UnderReview =>
                next is DevSphere.Domain.Enums.ApplicationStatus.Shortlisted or DevSphere.Domain.Enums.ApplicationStatus.Rejected,

            DevSphere.Domain.Enums.ApplicationStatus.Shortlisted =>
                next is DevSphere.Domain.Enums.ApplicationStatus.Selected or DevSphere.Domain.Enums.ApplicationStatus.Rejected,

            DevSphere.Domain.Enums.ApplicationStatus.Selected => false,

            DevSphere.Domain.Enums.ApplicationStatus.Rejected => false,

            _ => false
        };
    }

    internal static string ResolveApplyDecision(
        bool calculationFailure,
        bool accountStateBlocked,
        bool vacancyUnavailableOrSuppressed,
        bool relationshipBlocked,
        bool blockedByRegulatoryGate,
        bool blockedByReadiness,
        bool baselineAcknowledgementRequired)
    {
        if (calculationFailure)
            return "CalculationFailure";

        if (accountStateBlocked)
            return "AccountStateBlocked";

        if (vacancyUnavailableOrSuppressed)
            return "VacancyUnavailableOrSuppressed";

        if (relationshipBlocked)
            return "RelationshipBlocked";

        if (blockedByRegulatoryGate)
            return "BlockedByRegulatoryGate";

        if (blockedByReadiness)
            return "BlockedByReadiness";

        if (baselineAcknowledgementRequired)
            return "AllowedAfterBaselineAcknowledgement";

        return "Allowed";
    }
}
