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

    public JobApplicationService(
        JobApplicationRepository repository,
        INotificationService notificationService,
        IMatchEngine matchEngine,
        IApplicationHistoryService applicationHistoryService)
    {
        _repository = repository;
        _notificationService = notificationService;
        _matchEngine = matchEngine;
        _applicationHistoryService = applicationHistoryService;
    }


    public async Task<JobApplicationDto> ApplyAsync(
        JobApplicationDto request)
    {
        var exists = await _repository.ExistsAsync(
            request.CandidateId,
            request.VacancyId);


        if (exists)
        {
            throw new ApplicationConflictException("Application already exists.");
        }


        var application = new DevSphere.Domain.Entities.Applications.JobApplication
        {
            Id = Guid.NewGuid(),
            CandidateId = request.CandidateId,
            VacancyId = request.VacancyId,
            Status = DevSphere.Domain.Enums.ApplicationStatus.Applied,
            AppliedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };


        await _repository.AddAsync(application);


        return new JobApplicationDto
        {
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
                ApplicationId = application.Id,
                CandidateId = application.CandidateId,
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
            .OrderByDescending(x => x.RawCompatibility.HasValue)
            .ThenByDescending(x => x.RawCompatibility)
            .ThenByDescending(x => x.HighTierAggregate.HasValue)
            .ThenByDescending(x => x.HighTierAggregate)
            .ThenByDescending(x => x.MediumTierAggregate.HasValue)
            .ThenByDescending(x => x.MediumTierAggregate)
            .ThenBy(x => x.AppliedAt)
            .ThenBy(x => x.ApplicationId)
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
}
