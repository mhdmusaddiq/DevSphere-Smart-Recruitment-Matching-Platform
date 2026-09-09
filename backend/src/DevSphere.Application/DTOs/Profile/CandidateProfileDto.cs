namespace DevSphere.Application.DTOs.Profile;

public class CandidateProfileDto
{
    public string FullName { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int ExperienceMonths { get; set; }

    public string Education { get; set; } = string.Empty;

    public string PreferredWorkMode { get; set; } = string.Empty;

    public string PreferredLocation { get; set; } = string.Empty;

    public bool WillingToRelocate { get; set; }

    public string PreferredEmploymentType { get; set; } = string.Empty;

    public string AvailabilityStatus { get; set; } = string.Empty;

    public DateOnly? AvailableFrom { get; set; }

    public int? NoticePeriodDays { get; set; }
}
