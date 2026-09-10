using Microsoft.EntityFrameworkCore;
using DevSphere.Application.Exceptions;
using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Repositories;
using System.Data;
using System.Text.Json;

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
        ApplyApplicationRequest request)
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

        await using var applyTransaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        // Re-evaluate mutable eligibility inputs and duplicate state inside
        // the final persistence transaction. The client decision is advisory.
        if (await _repository.ExistsAsync(candidateId, request.VacancyId))
        {
            throw new ApplicationConflictException(
                "Application already exists.");
        }

        await _context.Entry(vacancy).ReloadAsync();

        MatchResultDto? match = null;
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

        var blockedByRegulatoryGate =
            match?.Families
                .SelectMany(x => x.Criteria)
                .Any(x =>
                    x.IsRegulatoryGate &&
                    x.State == MatchCriterionState.NotMet) == true;

        var baselineAcknowledgedRequired =
            !blockedByRegulatoryGate &&
            match?.Eligibility ==
                MatchEligibilityStatus.DoesNotMeetBaseline;

        blockedByReadiness =
            blockedByReadiness ||
            match?.Eligibility is
                MatchEligibilityStatus.IncompleteAssessment or
                MatchEligibilityStatus.PendingVerification;

        var vacancyUnavailableOrSuppressed =
            vacancy.LifecycleStatus !=
                DevSphere.Domain.Enums.VacancyLifecycleStatus.Published ||
            !vacancy.IsOpen ||
            (vacancy.ClosingDateUtc.HasValue &&
             vacancy.ClosingDateUtc.Value <= DateTime.UtcNow);

        calculationFailure =
            calculationFailure ||
            match?.AssessmentStatus ==
                MatchAssessmentStatus.CalculationFailure;

        var decision = await EvaluateApplyDecisionCoreAsync(
            candidateId,
            request.VacancyId);

        var isEligible = decision.CanSubmit;

        var rawCompatibilityScore =
            match?.RawCompatibility;

        decimal? displayCompatibilityScore =
            rawCompatibilityScore.HasValue
                ? decimal.Round(
                    rawCompatibilityScore.Value,
                    1,
                    MidpointRounding.AwayFromZero)
                : null;

        var compatibilityStatus =
            calculationFailure
                ? MatchAssessmentStatus.CalculationFailure.ToString()
                : (match?.AssessmentStatus ??
                    MatchAssessmentStatus.NotCalculated).ToString();

        var eligibilityStatus =
            (match?.Eligibility ??
                MatchEligibilityStatus.IncompleteAssessment).ToString();

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
            assessmentStatus = compatibilityStatus,
            eligibility = eligibilityStatus,
            eligibilityReason = match?.EligibilityReason,
            rawCompatibility = rawCompatibilityScore,
            displayCompatibility = displayCompatibilityScore,
            highTierAggregate = match?.HighTierAggregate,
            mediumTierAggregate = match?.MediumTierAggregate,
            coverage = match?.Coverage ?? 0m,
            missingInputs = match?.MissingInputs ?? new List<string>(),
            families = match?.Families ?? new List<MatchFamilyResultDto>(),
            applyDecision = decision
        };
        if (!isEligible)
        {
            throw new ApplicationConflictException(
                $"Application blocked: {decision.PrimaryCode}.");
        }

        if (decision.RequiresBaselineAcknowledgement &&
            !request.BaselineAcknowledged)
        {
            throw new ApplicationConflictException(
                "Baseline acknowledgement is required before applying.");
        }

        var now = DateTime.UtcNow;

        var application =
            new DevSphere.Domain.Entities.Applications.JobApplication
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                VacancyId = request.VacancyId,
                Status =
                    DevSphere.Domain.Enums.ApplicationStatus.Submitted,
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
                HighTierAggregateScore = match?.HighTierAggregate,
                MediumTierAggregateScore = match?.MediumTierAggregate,
                Coverage = match?.Coverage ?? 0m,
                EligibilityReason = match?.EligibilityReason,
                ApplyDecision = JsonSerializer.Serialize(decision),
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
                    JsonSerializer.Serialize(
                        evidenceSummary),
                MatchResultJson =
                    JsonSerializer.Serialize(
                        match ?? new MatchResultDto
                        {
                            AssessmentStatus =
                                MatchAssessmentStatus.NotCalculated,
                            Eligibility =
                                MatchEligibilityStatus.IncompleteAssessment,
                            EligibilityReason =
                                "Matching result unavailable."
                        }),
                CapturedAtUtc = now,
                CreatedAt = now
            };

        await _repository.AddWithSnapshotAsync(
            application,
            snapshot);

        await applyTransaction.CommitAsync();

        return new JobApplicationDto
        {
            Id = application.Id,
            CandidateId = application.CandidateId,
            VacancyId = application.VacancyId,
            Status = application.Status.ToString(),
            ApplyDecision = decision
        };
    }

    public async Task<ApplyDecisionDto> GetApplyDecisionAsync(
        string candidateId,
        Guid vacancyId)
    {
        if (await _repository.ExistsAsync(candidateId, vacancyId))
        {
            throw new ApplicationConflictException(
                "Application already exists.");
        }

        return await EvaluateApplyDecisionCoreAsync(
            candidateId,
            vacancyId);
    }

    private async Task<ApplyDecisionDto> EvaluateApplyDecisionCoreAsync(
        string candidateId,
        Guid vacancyId)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated candidate is required.");
        }

        if (vacancyId == Guid.Empty)
        {
            throw new ArgumentException("Vacancy is required.", nameof(vacancyId));
        }

        var accountIsActive = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == candidateId)
            .Select(x => (bool?)x.IsActive)
            .FirstOrDefaultAsync();
        var accountStateBlocked = accountIsActive != true;

        var candidate = await _candidateProfileRepository
            .GetByUserIdWithSkillsAsync(candidateId)
            ?? throw new InvalidOperationException(
                "Candidate profile is required before applying.");
        var vacancy = await _vacancyRepository.GetByIdAsync(vacancyId)
            ?? throw new KeyNotFoundException("Vacancy not found.");

        var resume = await _resumeRepository
            .GetByCandidateProfileIdAsync(candidate.Id);
        var currentResume = resume?.Versions
            .Where(x => x.IsCurrent)
            .OrderByDescending(x => x.VersionNumber)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();
        var selectedResumeVersion = resume != null &&
            currentResume != null &&
            resume.CurrentVersionId == currentResume.Id
                ? currentResume
                : null;

        MatchResultDto? match = null;
        var calculationFailure = false;

        try
        {
            match = await _matchEngine.CalculateAsync(
                candidateId,
                vacancyId.ToString());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            calculationFailure = true;
        }

        var currentPolicy = await _context.MatchingPolicyRevisions
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.VacancyId == vacancyId &&
                x.IsCurrent);
        var readiness = await _candidateProfileService
            .GetApplicationReadinessAsync(candidateId);
        var readinessMissingItems = readiness.MissingItems.ToList();
        var blockedByReadiness = !readiness.IsReady;

        if (selectedResumeVersion != null &&
            readiness.CurrentResumeVersionId.HasValue &&
            readiness.CurrentResumeVersionId.Value != selectedResumeVersion.Id)
        {
            blockedByReadiness = true;
            if (!readinessMissingItems.Contains("Selected resume version is stale."))
            {
                readinessMissingItems.Add("Selected resume version is stale.");
            }
        }

        if (selectedResumeVersion == null)
        {
            blockedByReadiness = true;
            if (!readinessMissingItems.Contains("Current resume version"))
            {
                readinessMissingItems.Add("Current resume version");
            }
        }

        var blockedByRegulatoryGate = match?.Families
            .SelectMany(x => x.Criteria)
            .Any(x =>
                x.IsRegulatoryGate &&
                x.State == MatchCriterionState.NotMet) == true;
        var relationshipBlocked = false;
        var baselineAcknowledgementRequired =
            !blockedByRegulatoryGate &&
            match?.Eligibility == MatchEligibilityStatus.DoesNotMeetBaseline;

        blockedByReadiness = blockedByReadiness ||
            match?.Eligibility is
                MatchEligibilityStatus.IncompleteAssessment or
                MatchEligibilityStatus.PendingVerification;

        var vacancyUnavailableOrSuppressed =
            vacancy.LifecycleStatus !=
                DevSphere.Domain.Enums.VacancyLifecycleStatus.Published ||
            !vacancy.IsOpen ||
            (vacancy.ClosingDateUtc.HasValue &&
             vacancy.ClosingDateUtc.Value <= DateTime.UtcNow);

        calculationFailure = calculationFailure ||
            match?.AssessmentStatus == MatchAssessmentStatus.CalculationFailure;

        var primaryCode = ResolveApplyDecision(
            calculationFailure,
            accountStateBlocked,
            vacancyUnavailableOrSuppressed,
            relationshipBlocked,
            blockedByRegulatoryGate,
            blockedByReadiness,
            baselineAcknowledgementRequired);
        var reasonCodes = new List<string>();

        AddReasonIf(calculationFailure, "CalculationFailure", reasonCodes);
        AddReasonIf(accountStateBlocked, "AccountStateBlocked", reasonCodes);
        AddReasonIf(vacancyUnavailableOrSuppressed, "VacancyUnavailableOrSuppressed", reasonCodes);
        AddReasonIf(relationshipBlocked, "RelationshipBlocked", reasonCodes);
        AddReasonIf(blockedByRegulatoryGate, "BlockedByRegulatoryGate", reasonCodes);
        AddReasonIf(blockedByReadiness, "BlockedByReadiness", reasonCodes);
        AddReasonIf(baselineAcknowledgementRequired, "AllowedAfterBaselineAcknowledgement", reasonCodes);

        return CreateApplyDecision(
            primaryCode,
            reasonCodes,
            vacancyId,
            currentPolicy?.Id,
            currentPolicy?.RevisionNumber,
            selectedResumeVersion?.Id,
            readinessMissingItems,
            match?.EligibilityReason);
    }

    private static void AddReasonIf(
        bool condition,
        string code,
        ICollection<string> reasons)
    {
        if (condition)
        {
            reasons.Add(code);
        }
    }

    public async Task<List<JobApplicationDto>> GetByCandidateAsync(
        string candidateId)
    {
        var applications = (await _repository
            .GetByCandidateAsync(candidateId)).ToList();

        var companyIds = applications
            .Where(x => x.Vacancy.CompanyId.HasValue)
            .Select(x => x.Vacancy.CompanyId!.Value)
            .Distinct()
            .ToList();

        var companyNames = await _context.CompanyProfiles
            .AsNoTracking()
            .Where(x => companyIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name);

        return applications
            .Select(x => new JobApplicationDto
            {
                Id = x.Id,
                CandidateId = x.CandidateId,
                VacancyId = x.VacancyId,
                VacancyTitle = x.Vacancy.Title,
                VacancyLocation = x.Vacancy.Location,
                WorkMode = x.Vacancy.WorkMode,
                CompanyId = x.Vacancy.CompanyId,
                CompanyName = x.Vacancy.CompanyId.HasValue &&
                    companyNames.TryGetValue(
                        x.Vacancy.CompanyId.Value,
                        out var companyName)
                            ? companyName
                            : string.Empty,
                SubmittedAtUtc = x.AppliedAt,
                Status = x.Status.ToString(),
                FrozenAssessmentStatus =
                    x.Snapshot?.CompatibilityStatus ??
                    MatchAssessmentStatus.NotCalculated.ToString(),
                DisplayCompatibility =
                    x.Snapshot?.DisplayCompatibilityScore,
                Eligibility =
                    x.Snapshot?.EligibilityStatus ??
                    MatchEligibilityStatus.IncompleteAssessment.ToString(),
                ResumeVersionId = x.Snapshot?.ResumeVersionId,
                CapturedAtUtc = x.Snapshot?.CapturedAtUtc
            })
            .ToList();
    }

    public async Task<List<RankedApplicantDto>> GetByVacancyAsync(
        Guid vacancyId,
        string employerId)
    {
        var applications = await _repository
            .GetByVacancyAsync(vacancyId, employerId);

        var candidateIds = applications
            .Select(x => x.CandidateId)
            .Distinct()
            .ToList();

        var candidateNames = await _context.CandidateProfiles
            .AsNoTracking()
            .Where(x => candidateIds.Contains(x.UserId))
            .ToDictionaryAsync(x => x.UserId, x => x.FullName);

        var rankedApplicants = new List<RankedApplicantDto>();

        foreach (var application in applications)
        {
            var match = ReadSnapshotMatch(application.Snapshot);

            rankedApplicants.Add(new RankedApplicantDto
            {
                ApplicationId = application.Id,
                CandidateId = application.CandidateId,
                CandidateDisplayName = candidateNames.TryGetValue(
                    application.CandidateId,
                    out var candidateName)
                        ? candidateName
                        : string.Empty,
                VacancyId = application.VacancyId,
                AppliedAt = application.AppliedAt,
                Status = application.Status.ToString(),
                RawCompatibility = match.RawCompatibility,
                MatchScore = match.DisplayCompatibility,
                AssessmentStatus = match.AssessmentStatus,
                Eligibility = match.Eligibility,
                EligibilityReason = match.EligibilityReason,
                HighTierAggregate = match.HighTierAggregate,
                MediumTierAggregate = match.MediumTierAggregate,
                Coverage = match.Coverage,
                MatchedSkills = match.MatchedSkills.ToList(),
                MissingSkills = match.MissingSkills.ToList(),
                MissingInputs = match.MissingInputs.ToList(),
                Families = match.Families
                    .Select(family => new MatchFamilyResultDto
                    {
                        Family = family.Family,
                        Importance = family.Importance,
                        RawScore = family.RawScore,
                        DisplayScore = family.DisplayScore,
                        Criteria = family.Criteria
                            .Select(criterion => new MatchCriterionResultDto
                            {
                                RequirementId = criterion.RequirementId,
                                AlternativeSetId = criterion.AlternativeSetId,
                                Family = criterion.Family,
                                Mode = criterion.Mode,
                                Importance = criterion.Importance,
                                State = criterion.State,
                                Score = criterion.Score,
                                IsRegulatoryGate = criterion.IsRegulatoryGate,
                                Label = criterion.Label
                            })
                            .ToList()
                    })
                    .ToList()
            });
        }

        return rankedApplicants
            .OrderBy(x => GetEligibilityRank(x.Eligibility))
            .ThenBy(x => GetAssessmentRank(x.AssessmentStatus))
            .ThenByDescending(x => x.RawCompatibility)
            .ThenByDescending(x => x.HighTierAggregate)
            .ThenByDescending(x => x.MediumTierAggregate)
            .ThenBy(x => x.AppliedAt)
            .ThenBy(x => x.ApplicationId)
            .ToList();
    }

    public async Task<JobApplicationDto?> GetCandidateApplicationAsync(
        Guid applicationId,
        string candidateId)
    {
        return (await GetByCandidateAsync(candidateId))
            .SingleOrDefault(x => x.Id == applicationId);
    }

    public async Task<EmployerApplicationDetailDto>
        GetEmployerApplicationAsync(
            Guid applicationId,
            string employerId)
    {
        var application = await _context.JobApplications
            .AsNoTracking()
            .Include(x => x.Vacancy)
            .Include(x => x.Snapshot)
            .FirstOrDefaultAsync(x => x.Id == applicationId)
            ?? throw new KeyNotFoundException("Application not found.");

        if (application.Vacancy.EmployerId != employerId)
        {
            throw new UnauthorizedAccessException(
                "Employer does not own this application.");
        }

        var candidateDisplayName = await _context.CandidateProfiles
            .AsNoTracking()
            .Where(x => x.UserId == application.CandidateId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync() ?? string.Empty;
        var match = ReadSnapshotMatch(application.Snapshot);

        return new EmployerApplicationDetailDto
        {
            ApplicationId = application.Id,
            CandidateId = application.CandidateId,
            CandidateDisplayName = candidateDisplayName,
            VacancyId = application.VacancyId,
            VacancyTitle = application.Vacancy.Title,
            SubmittedAtUtc = application.AppliedAt,
            Status = application.Status.ToString(),
            AssessmentStatus = match.AssessmentStatus,
            DisplayCompatibility = match.DisplayCompatibility,
            Eligibility = match.Eligibility,
            EligibilityReason = match.EligibilityReason,
            MatchedSkills = match.MatchedSkills.ToList(),
            MissingSkills = match.MissingSkills.ToList(),
            MissingInputs = match.MissingInputs.ToList(),
            Families = match.Families.ToList(),
            ResumeVersionId = application.Snapshot?.ResumeVersionId,
            CapturedAtUtc = application.Snapshot?.CapturedAtUtc
        };
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
            .GetWithVacancyAsync(applicationId);

        if (application == null)
        {
            throw new KeyNotFoundException(
                "Application not found.");
        }

        if (application.Vacancy.EmployerId != changedByUserId)
        {
            throw new UnauthorizedAccessException(
                "Employer does not own this application.");
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

        await PersistStatusTransitionAsync(
            application,
            previousStatus,
            newStatus,
            changedByUserId,
            notifyCandidate: true);

        return new JobApplicationDto
        {
            Id = application.Id,
            CandidateId = application.CandidateId,
            VacancyId = application.VacancyId,
            Status = application.Status.ToString()
        };
    }

    public async Task<JobApplicationDto> WithdrawAsync(
        Guid applicationId,
        string candidateId)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated candidate is required.");
        }

        var application = await _repository
            .GetByIdAsync(applicationId);

        if (application == null ||
            application.CandidateId != candidateId)
        {
            throw new KeyNotFoundException(
                "Application not found.");
        }

        var previousStatus = application.Status;

        if (!CanCandidateWithdraw(previousStatus))
        {
            throw new InvalidOperationException(
                $"Application cannot be withdrawn from {previousStatus}.");
        }

        var withdrawn =
            DevSphere.Domain.Enums.ApplicationStatus.Withdrawn;

        await PersistStatusTransitionAsync(
            application,
            previousStatus,
            withdrawn,
            candidateId,
            notifyCandidate: false);

        return new JobApplicationDto
        {
            Id = application.Id,
            CandidateId = application.CandidateId,
            VacancyId = application.VacancyId,
            Status = application.Status.ToString()
        };
    }

    private async Task PersistStatusTransitionAsync(
        DevSphere.Domain.Entities.Applications.JobApplication application,
        DevSphere.Domain.Enums.ApplicationStatus previousStatus,
        DevSphere.Domain.Enums.ApplicationStatus newStatus,
        string changedByUserId,
        bool notifyCandidate)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        await _context.Entry(application).ReloadAsync();

        if (application.Status != previousStatus)
        {
            throw new InvalidOperationException(
                "The application status changed; refresh and retry.");
        }

        var changedAt = DateTime.UtcNow;
        application.Status = newStatus;
        application.UpdatedAt = changedAt;

        _context.ApplicationStatusHistories.Add(
            new DevSphere.Domain.Entities.Applications.ApplicationStatusHistory
            {
                Id = Guid.NewGuid(),
                JobApplicationId = application.Id,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                ChangedByUserId = changedByUserId,
                ChangedAtUtc = changedAt,
                Notes =
                    $"Status changed from {previousStatus} to {newStatus}.",
                CreatedAt = changedAt
            });

        if (notifyCandidate)
        {
            _context.Notifications.Add(
                new DevSphere.Domain.Entities.Notifications.Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = application.CandidateId,
                    Message =
                        $"Your application status has been updated to {newStatus}.",
                    IsRead = false,
                    CreatedAtUtc = changedAt,
                    CreatedAt = changedAt
                });
        }

        if (newStatus is
            DevSphere.Domain.Enums.ApplicationStatus.Rejected or
            DevSphere.Domain.Enums.ApplicationStatus.Withdrawn)
        {
            var pendingContacts = await _context.ContactRequests
                .Where(x =>
                    x.JobApplicationId == application.Id &&
                    x.Status == "Pending")
                .ToListAsync();

            foreach (var contact in pendingContacts)
            {
                contact.Status = "Cancelled";
                contact.UpdatedAt = changedAt;
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static bool CanCandidateWithdraw(
        DevSphere.Domain.Enums.ApplicationStatus current)
    {
        return current is
            DevSphere.Domain.Enums.ApplicationStatus.Submitted or
            DevSphere.Domain.Enums.ApplicationStatus.Screening or
            DevSphere.Domain.Enums.ApplicationStatus.UnderReview or
            DevSphere.Domain.Enums.ApplicationStatus.Shortlisted;
    }

    public static bool IsValidTransition(
        DevSphere.Domain.Enums.ApplicationStatus current,
        DevSphere.Domain.Enums.ApplicationStatus next)
    {
        return current switch
        {
            DevSphere.Domain.Enums.ApplicationStatus.Submitted =>
                next is
                    DevSphere.Domain.Enums.ApplicationStatus.Screening or
                    DevSphere.Domain.Enums.ApplicationStatus.UnderReview or
                    DevSphere.Domain.Enums.ApplicationStatus.Shortlisted or
                    DevSphere.Domain.Enums.ApplicationStatus.Rejected,

            DevSphere.Domain.Enums.ApplicationStatus.Screening =>
                next is
                    DevSphere.Domain.Enums.ApplicationStatus.UnderReview or
                    DevSphere.Domain.Enums.ApplicationStatus.Shortlisted or
                    DevSphere.Domain.Enums.ApplicationStatus.Rejected,

            DevSphere.Domain.Enums.ApplicationStatus.UnderReview =>
                next is DevSphere.Domain.Enums.ApplicationStatus.Shortlisted or DevSphere.Domain.Enums.ApplicationStatus.Rejected,

            DevSphere.Domain.Enums.ApplicationStatus.Shortlisted =>
                next is DevSphere.Domain.Enums.ApplicationStatus.Selected or DevSphere.Domain.Enums.ApplicationStatus.Rejected,

            DevSphere.Domain.Enums.ApplicationStatus.Selected => false,

            DevSphere.Domain.Enums.ApplicationStatus.Rejected => false,

            DevSphere.Domain.Enums.ApplicationStatus.Withdrawn => false,

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

    private static ApplyDecisionDto CreateApplyDecision(
        string primaryCode,
        IReadOnlyCollection<string> reasonCodes,
        Guid vacancyId,
        Guid? matchingPolicyRevisionId,
        int? matchingPolicyRevisionNumber,
        Guid? resumeVersionId,
        IReadOnlyCollection<string> readinessMissingItems,
        string? eligibilityReason)
    {
        var canSubmit =
            primaryCode is
                "Allowed" or
                "AllowedAfterBaselineAcknowledgement";

        var reasons = new List<ApplyDecisionReasonDto>();

        foreach (var reasonCode in reasonCodes)
        {
            reasons.Add(new ApplyDecisionReasonDto
            {
                Code = reasonCode,
                Message = GetApplyDecisionMessage(
                    reasonCode,
                    readinessMissingItems,
                    eligibilityReason),
                TargetCta = GetApplyDecisionTargetCta(reasonCode)
            });
        }

        return new ApplyDecisionDto
        {
            CanSubmit = canSubmit,
            RequiresBaselineAcknowledgement =
                primaryCode ==
                    "AllowedAfterBaselineAcknowledgement",
            PrimaryCode = primaryCode,
            Reasons = reasons,
            EvaluatedAtUtc = DateTime.UtcNow,
            VacancyId = vacancyId,
            MatchingPolicyRevisionId = matchingPolicyRevisionId,
            MatchingPolicyRevisionNumber = matchingPolicyRevisionNumber,
            ResumeVersionId = resumeVersionId
        };
    }

    private static string GetApplyDecisionMessage(
        string primaryCode,
        IReadOnlyCollection<string> readinessMissingItems,
        string? eligibilityReason)
    {
        return primaryCode switch
        {
            "CalculationFailure" =>
                "Matching could not be evaluated. Try again.",
            "AccountStateBlocked" =>
                "Your account is not active.",
            "VacancyUnavailableOrSuppressed" =>
                "This vacancy is not available for applications.",
            "RelationshipBlocked" =>
                "This application is not permitted.",
            "BlockedByRegulatoryGate" =>
                "A required regulatory condition is not met.",
            "BlockedByReadiness" =>
                readinessMissingItems.Count > 0
                    ? $"Complete application readiness: {string.Join(", ", readinessMissingItems)}."
                    : eligibilityReason ??
                        "Complete the required candidate information.",
            "AllowedAfterBaselineAcknowledgement" =>
                eligibilityReason ??
                    "You may apply after acknowledging the baseline mismatch.",
            _ => "Application is allowed."
        };
    }

    private static string GetApplyDecisionTargetCta(string primaryCode)
    {
        return primaryCode switch
        {
            "CalculationFailure" => "Retry",
            "AccountStateBlocked" => "ContactSupport",
            "VacancyUnavailableOrSuppressed" => "ViewJobs",
            "RelationshipBlocked" => "ViewJobs",
            "BlockedByRegulatoryGate" => "ReviewRequirements",
            "BlockedByReadiness" => "CompleteProfile",
            "AllowedAfterBaselineAcknowledgement" =>
                "AcknowledgeAndApply",
            _ => "Apply"
        };
    }

    private static MatchResultDto ReadSnapshotMatch(
        DevSphere.Domain.Entities.Applications.ApplicationSnapshot? snapshot)
    {
        if (snapshot == null)
        {
            return new MatchResultDto
            {
                AssessmentStatus =
                    MatchAssessmentStatus.NotCalculated,
                Eligibility =
                    MatchEligibilityStatus.IncompleteAssessment,
                EligibilityReason =
                    "Application snapshot is unavailable."
            };
        }

        if (!string.IsNullOrWhiteSpace(snapshot.MatchResultJson))
        {
            try
            {
                var stored = JsonSerializer.Deserialize<MatchResultDto>(
                    snapshot.MatchResultJson);

                if (stored != null)
                {
                    return stored;
                }
            }
            catch (JsonException)
            {
                // Fall back to the explicit immutable snapshot columns.
            }
        }

        Enum.TryParse(
            snapshot.CompatibilityStatus,
            true,
            out MatchAssessmentStatus assessmentStatus);
        Enum.TryParse(
            snapshot.EligibilityStatus,
            true,
            out MatchEligibilityStatus eligibility);

        return new MatchResultDto
        {
            AssessmentStatus = assessmentStatus == 0
                ? MatchAssessmentStatus.NotCalculated
                : assessmentStatus,
            Eligibility = eligibility == 0
                ? MatchEligibilityStatus.IncompleteAssessment
                : eligibility,
            EligibilityReason = snapshot.EligibilityReason,
            RawCompatibility = snapshot.RawCompatibilityScore,
            DisplayCompatibility = snapshot.DisplayCompatibilityScore,
            HighTierAggregate = snapshot.HighTierAggregateScore,
            MediumTierAggregate = snapshot.MediumTierAggregateScore,
            Coverage = snapshot.Coverage,
            MatchedSkills = DeserializeStringList(
                snapshot.MatchedSkillsJson),
            MissingSkills = DeserializeStringList(
                snapshot.GapSkillsJson)
        };
    }

    private static List<string> DeserializeStringList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ??
                new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private static int GetEligibilityRank(
        MatchEligibilityStatus eligibility)
    {
        return eligibility switch
        {
            MatchEligibilityStatus.MeetsBaseline => 0,
            MatchEligibilityStatus.PendingVerification => 1,
            MatchEligibilityStatus.IncompleteAssessment => 2,
            MatchEligibilityStatus.DoesNotMeetBaseline => 3,
            _ => 4
        };
    }

    private static int GetAssessmentRank(
        MatchAssessmentStatus assessmentStatus)
    {
        return assessmentStatus switch
        {
            MatchAssessmentStatus.Calculated => 0,
            MatchAssessmentStatus.Provisional => 1,
            MatchAssessmentStatus.NotCalculated => 2,
            MatchAssessmentStatus.CalculationFailure => 3,
            _ => 4
        };
    }
}
