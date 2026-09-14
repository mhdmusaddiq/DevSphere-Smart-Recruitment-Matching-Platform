namespace DevSphere.Domain.Entities.Vacancies;

public class MatchingPolicyRevision : BaseEntity
{
    public Guid VacancyId { get; set; }

    public int RevisionNumber { get; set; }

    public bool IsCurrent { get; set; } = true;

    public bool IsMateriallyLocked { get; set; }

    public DateTime? MateriallyLockedAtUtc { get; set; }
}
