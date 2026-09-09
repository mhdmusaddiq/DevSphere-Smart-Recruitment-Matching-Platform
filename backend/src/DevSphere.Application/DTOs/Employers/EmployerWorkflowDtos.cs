namespace DevSphere.Application.DTOs.Employers;

public class CreateInterviewRequest
{
    public Guid JobApplicationId { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class InterviewDto
{
    public Guid Id { get; set; }
    public Guid JobApplicationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class CreateInterviewSlotRequest
{
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public string LocationOrMeetingUrl { get; set; } = string.Empty;
}

public class InterviewSlotDto
{
    public Guid Id { get; set; }
    public Guid InterviewId { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public string LocationOrMeetingUrl { get; set; } = string.Empty;
}

public class CreateScorecardRequest
{
    public Guid JobApplicationId { get; set; }
    public Guid? InterviewId { get; set; }
    public int OverallRating { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class ScorecardDto
{
    public Guid Id { get; set; }
    public Guid JobApplicationId { get; set; }
    public Guid? InterviewId { get; set; }
    public int OverallRating { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CreateOfferRequest
{
    public Guid JobApplicationId { get; set; }
    public decimal? OfferedSalary { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class UpdateOfferStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class OfferDto
{
    public Guid Id { get; set; }
    public Guid JobApplicationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? OfferedSalary { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? ExtendedAtUtc { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CreateTalentPoolEntryRequest
{
    public Guid JobApplicationId { get; set; }
    public bool HasCandidateConsent { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class TalentPoolEntryDto
{
    public Guid Id { get; set; }
    public Guid JobApplicationId { get; set; }
    public string CandidateUserId { get; set; } = string.Empty;
    public bool HasCandidateConsent { get; set; }
    public DateTime? ConsentRecordedAtUtc { get; set; }
    public bool IsActive { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class UpdateInterviewStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class EmployerWorkflowSummaryDto
{
    public Guid JobApplicationId { get; set; }
    public List<InterviewDto> Interviews { get; set; } = new();
    public List<InterviewSlotDto> InterviewSlots { get; set; } = new();
    public List<ScorecardDto> Scorecards { get; set; } = new();
    public List<OfferDto> Offers { get; set; } = new();
    public List<TalentPoolEntryDto> TalentPoolEntries { get; set; } = new();
}
