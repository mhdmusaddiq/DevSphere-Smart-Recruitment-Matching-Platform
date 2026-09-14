namespace DevSphere.Application.DTOs.Profile;

public class VacancyDto
{
    public Guid Id { get; set; }

    public Guid? CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string CompanyVerificationStatus { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string WorkMode { get; set; } = string.Empty;

    public string EmploymentType { get; set; } = string.Empty;

    public int RequiredExperienceMonths { get; set; }

    public int MinExperienceMonths { get; set; }

    public int? MaxExperienceMonths { get; set; }

    public string RequiredEducation { get; set; } = string.Empty;

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public DateTime? ClosingDateUtc { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public List<VacancyRequiredSkillDto> RequiredSkills { get; set; } = new();

    public string LifecycleStatus { get; set; } = string.Empty;

    // Compatibility projection:
    // Draft = false, Published = true, Closed = false.
    public bool IsOpen { get; set; }

    public string AssessmentStatus { get; set; } = string.Empty;

    public string Eligibility { get; set; } = string.Empty;

    public decimal? RawCompatibility { get; set; }

    public decimal? DisplayCompatibility { get; set; }

    public decimal? HighTierAggregate { get; set; }

    public decimal? MediumTierAggregate { get; set; }

    public decimal? Coverage { get; set; }
}

public class VacancyRequiredSkillDto
{
    public string Name { get; set; } = string.Empty;

    public int Weight { get; set; }
}
