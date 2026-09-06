namespace DevSphere.Domain.Entities.Vacancies;

public class RequiredSkill : BaseEntity
{
    public Guid VacancyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? MinimumExperienceMonths { get; set; }
}
