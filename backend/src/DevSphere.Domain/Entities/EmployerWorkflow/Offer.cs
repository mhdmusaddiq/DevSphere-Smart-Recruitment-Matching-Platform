using DevSphere.Domain.Enums;

namespace DevSphere.Domain.Entities.EmployerWorkflow;

public class Offer : BaseEntity
{
    public Guid JobApplicationId { get; set; }

    public string EmployerUserId { get; set; } = string.Empty;

    public OfferStatus Status { get; set; }
        = OfferStatus.Draft;

    public decimal? OfferedSalary { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public DateTime? ExtendedAtUtc { get; set; }

    public string Notes { get; set; } = string.Empty;
}
