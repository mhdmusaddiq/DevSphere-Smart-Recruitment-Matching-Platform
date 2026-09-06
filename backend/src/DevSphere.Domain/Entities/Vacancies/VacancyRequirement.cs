namespace DevSphere.Domain.Entities.Vacancies;

public class VacancyRequirement : BaseEntity
{
    public Guid VacancyId { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsMandatory { get; set; }
}
