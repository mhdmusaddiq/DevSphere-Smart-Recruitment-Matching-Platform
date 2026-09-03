namespace DevSphere.Domain.Entities.Vacancies;

public class Vacancy : BaseEntity
{
    public string EmployerId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int RequiredExperienceMonths { get; set; }

    public bool IsOpen { get; set; } = true;
}