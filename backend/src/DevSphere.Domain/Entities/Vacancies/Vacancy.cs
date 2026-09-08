using DevSphere.Domain.Enums;

using DevSphere.Domain.Entities.Skills;

namespace DevSphere.Domain.Entities.Vacancies;

public class Vacancy : BaseEntity
{
    public string EmployerId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int RequiredExperienceMonths { get; set; }

    public int MinExperienceMonths { get; set; }

    public int? MaxExperienceMonths { get; set; }

    public string RequiredEducation { get; set; } = string.Empty;

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public DateTime? ClosingDateUtc { get; set; }

    public VacancyLifecycleStatus LifecycleStatus { get; set; }
        = VacancyLifecycleStatus.Draft;

    // Compatibility projection retained while the existing API still uses IsOpen.
    public bool IsOpen { get; set; } = false;

    public ICollection<Skill> RequiredSkills { get; set; } = new List<Skill>();
}
