using DevSphere.Application.DTOs.Employers;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Entities.EmployerWorkflow;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Employers;

public class EmployerWorkflowService :
    IEmployerWorkflowService
{
    private readonly EmployerWorkflowRepository _repository;

    public EmployerWorkflowService(
        EmployerWorkflowRepository repository)
    {
        _repository = repository;
    }

    public async Task<InterviewDto> CreateInterviewAsync(
        string employerUserId,
        CreateInterviewRequest request)
    {
        var application =
            await GetOwnedApplicationAsync(
                employerUserId,
                request.JobApplicationId);

        var interview = new Interview
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.Id,
            EmployerUserId = employerUserId,
            Status = InterviewStatus.Scheduled,
            Notes = request.Notes?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddInterviewAsync(interview);

        return MapInterview(interview);
    }

    public async Task<InterviewDto>
        UpdateInterviewStatusAsync(
            string employerUserId,
            Guid interviewId,
            UpdateInterviewStatusRequest request)
    {
        var interview =
            await _repository.GetInterviewAsync(
                interviewId);

        if (interview == null)
        {
            throw new KeyNotFoundException(
                "Interview not found.");
        }

        if (interview.EmployerUserId !=
            employerUserId)
        {
            throw new UnauthorizedAccessException(
                "You cannot manage this interview.");
        }

        if (!Enum.TryParse<InterviewStatus>(
            request.Status,
            true,
            out var nextStatus))
        {
            throw new ArgumentException(
                "Invalid interview status.");
        }

        if (interview.Status !=
                InterviewStatus.Scheduled ||
            (nextStatus !=
                InterviewStatus.Completed &&
             nextStatus !=
                InterviewStatus.Cancelled))
        {
            throw new InvalidOperationException(
                $"Illegal interview transition: {interview.Status} -> {nextStatus}.");
        }

        interview.Status = nextStatus;
        interview.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync();

        return MapInterview(interview);
    }
    public async Task<InterviewSlotDto>
        AddInterviewSlotAsync(
            string employerUserId,
            Guid interviewId,
            CreateInterviewSlotRequest request)
    {
        var interview =
            await _repository.GetInterviewAsync(
                interviewId);

        if (interview == null)
        {
            throw new KeyNotFoundException(
                "Interview not found.");
        }

        if (interview.EmployerUserId !=
            employerUserId)
        {
            throw new UnauthorizedAccessException(
                "You cannot manage this interview.");
        }

        if (interview.Status !=
            InterviewStatus.Scheduled)
        {
            throw new InvalidOperationException(
                "Slots can only be added to scheduled interviews.");
        }

        if (request.EndsAtUtc <=
            request.StartsAtUtc)
        {
            throw new ArgumentException(
                "Interview slot end time must be after start time.");
        }

        if (request.StartsAtUtc <=
            DateTime.UtcNow)
        {
            throw new ArgumentException(
                "Interview slot must start in the future.");
        }

        var slot = new InterviewSlot
        {
            Id = Guid.NewGuid(),
            InterviewId = interview.Id,
            StartsAtUtc = request.StartsAtUtc,
            EndsAtUtc = request.EndsAtUtc,
            LocationOrMeetingUrl =
                request.LocationOrMeetingUrl?.Trim()
                ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _repository
            .AddInterviewSlotAsync(slot);

        return new InterviewSlotDto
        {
            Id = slot.Id,
            InterviewId = slot.InterviewId,
            StartsAtUtc = slot.StartsAtUtc,
            EndsAtUtc = slot.EndsAtUtc,
            LocationOrMeetingUrl =
                slot.LocationOrMeetingUrl
        };
    }

    public async Task<ScorecardDto>
        CreateScorecardAsync(
            string employerUserId,
            CreateScorecardRequest request)
    {
        var application =
            await GetOwnedApplicationAsync(
                employerUserId,
                request.JobApplicationId);

        if (request.OverallRating < 1 ||
            request.OverallRating > 5)
        {
            throw new ArgumentException(
                "Overall rating must be between 1 and 5.");
        }

        if (request.InterviewId.HasValue)
        {
            var interview =
                await _repository.GetInterviewAsync(
                    request.InterviewId.Value);

            if (interview == null)
            {
                throw new KeyNotFoundException(
                    "Interview not found.");
            }

            if (interview.EmployerUserId !=
                    employerUserId ||
                interview.JobApplicationId !=
                    application.Id)
            {
                throw new UnauthorizedAccessException(
                    "Interview does not belong to this application and employer.");
            }

            if (interview.Status !=
                InterviewStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Interview must be completed before creating its scorecard.");
            }
        }

        var scorecard = new Scorecard
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.Id,
            InterviewId = request.InterviewId,
            AssessorEmployerUserId =
                employerUserId,
            OverallRating =
                request.OverallRating,
            Notes =
                request.Notes?.Trim()
                ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _repository
            .AddScorecardAsync(scorecard);

        return new ScorecardDto
        {
            Id = scorecard.Id,
            JobApplicationId =
                scorecard.JobApplicationId,
            InterviewId =
                scorecard.InterviewId,
            OverallRating =
                scorecard.OverallRating,
            Notes =
                scorecard.Notes
        };
    }

    public async Task<OfferDto> CreateOfferAsync(
        string employerUserId,
        CreateOfferRequest request)
    {
        var application =
            await GetOwnedApplicationAsync(
                employerUserId,
                request.JobApplicationId);

        if (request.OfferedSalary.HasValue &&
            request.OfferedSalary.Value < 0)
        {
            throw new ArgumentException(
                "Offered salary cannot be negative.");
        }

        if (request.ExpiresAtUtc.HasValue &&
            request.ExpiresAtUtc.Value <=
            DateTime.UtcNow)
        {
            throw new ArgumentException(
                "Offer expiry must be in the future.");
        }

        var offer = new Offer
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.Id,
            EmployerUserId = employerUserId,
            Status = OfferStatus.Draft,
            OfferedSalary = request.OfferedSalary,
            ExpiresAtUtc = request.ExpiresAtUtc,
            Notes =
                request.Notes?.Trim()
                ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddOfferAsync(offer);

        return MapOffer(offer);
    }

    public async Task<OfferDto>
        UpdateOfferStatusAsync(
            string employerUserId,
            Guid offerId,
            UpdateOfferStatusRequest request)
    {
        var offer =
            await _repository.GetOfferAsync(
                offerId);

        if (offer == null)
        {
            throw new KeyNotFoundException(
                "Offer not found.");
        }

        if (offer.EmployerUserId !=
            employerUserId)
        {
            throw new UnauthorizedAccessException(
                "You cannot manage this offer.");
        }

        if (!Enum.TryParse<OfferStatus>(
            request.Status,
            true,
            out var nextStatus))
        {
            throw new ArgumentException(
                "Invalid offer status.");
        }

        if (!IsValidOfferTransition(
            offer.Status,
            nextStatus))
        {
            throw new InvalidOperationException(
                $"Illegal offer transition: {offer.Status} -> {nextStatus}.");
        }

        offer.Status = nextStatus;

        if (nextStatus ==
            OfferStatus.Extended)
        {
            if (offer.ExpiresAtUtc.HasValue &&
                offer.ExpiresAtUtc.Value <=
                DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "Expired offers cannot be extended.");
            }

            offer.ExtendedAtUtc =
                DateTime.UtcNow;
        }

        offer.UpdatedAt =
            DateTime.UtcNow;

        await _repository.SaveAsync();

        return MapOffer(offer);
    }

    public async Task<TalentPoolEntryDto>
        AddTalentPoolEntryAsync(
            string employerUserId,
            CreateTalentPoolEntryRequest request)
    {
        var application =
            await GetOwnedApplicationAsync(
                employerUserId,
                request.JobApplicationId);

        if (!request.HasCandidateConsent)
        {
            throw new InvalidOperationException(
                "Candidate consent is required before adding a talent-pool entry.");
        }

        if (await _repository
            .TalentPoolEntryExistsAsync(
                application.Id,
                employerUserId))
        {
            throw new InvalidOperationException(
                "Talent-pool entry already exists for this application.");
        }

        var entry = new TalentPoolEntry
        {
            Id = Guid.NewGuid(),
            JobApplicationId =
                application.Id,
            CandidateUserId =
                application.CandidateId,
            EmployerUserId =
                employerUserId,
            HasCandidateConsent = true,
            ConsentRecordedAtUtc =
                DateTime.UtcNow,
            IsActive = true,
            Notes =
                request.Notes?.Trim()
                ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _repository
            .AddTalentPoolEntryAsync(entry);

        return new TalentPoolEntryDto
        {
            Id = entry.Id,
            JobApplicationId =
                entry.JobApplicationId,
            CandidateUserId =
                entry.CandidateUserId,
            HasCandidateConsent =
                entry.HasCandidateConsent,
            ConsentRecordedAtUtc =
                entry.ConsentRecordedAtUtc,
            IsActive =
                entry.IsActive,
            Notes =
                entry.Notes
        };
    }

    private async Task<JobApplication>
        GetOwnedApplicationAsync(
            string employerUserId,
            Guid applicationId)
    {
        if (string.IsNullOrWhiteSpace(
            employerUserId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated employer is required.");
        }

        var application =
            await _repository
                .GetApplicationWithVacancyAsync(
                    applicationId);

        if (application == null)
        {
            throw new KeyNotFoundException(
                "Application not found.");
        }

        if (application.Vacancy == null ||
            application.Vacancy.EmployerId !=
            employerUserId)
        {
            throw new UnauthorizedAccessException(
                "You cannot manage workflow for this application.");
        }

        return application;
    }

    private static bool IsValidOfferTransition(
        OfferStatus current,
        OfferStatus next)
    {
        if (current == next)
        {
            return false;
        }

        return current switch
        {
            OfferStatus.Draft =>
                next == OfferStatus.Extended ||
                next == OfferStatus.Withdrawn,

            OfferStatus.Extended =>
                next == OfferStatus.Accepted ||
                next == OfferStatus.Declined ||
                next == OfferStatus.Withdrawn,

            OfferStatus.Accepted => false,
            OfferStatus.Declined => false,
            OfferStatus.Withdrawn => false,

            _ => false
        };
    }

    private static InterviewDto MapInterview(
        Interview interview)
    {
        return new InterviewDto
        {
            Id = interview.Id,
            JobApplicationId =
                interview.JobApplicationId,
            Status =
                interview.Status.ToString(),
            Notes =
                interview.Notes
        };
    }

    private static OfferDto MapOffer(
        Offer offer)
    {
        return new OfferDto
        {
            Id = offer.Id,
            JobApplicationId =
                offer.JobApplicationId,
            Status =
                offer.Status.ToString(),
            OfferedSalary =
                offer.OfferedSalary,
            ExpiresAtUtc =
                offer.ExpiresAtUtc,
            ExtendedAtUtc =
                offer.ExtendedAtUtc,
            Notes =
                offer.Notes
        };
    }
}


