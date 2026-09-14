namespace DevSphere.Domain.Entities.Employers;

public class CompanyVerification : BaseEntity
{
    // Professional company aggregate.
    // Nullable during compatibility with the existing EmployerProfile flow.
    public Guid? CompanyId { get; set; }

    // Existing BRD/submission-core relationship kept for compatibility.
    public Guid EmployerProfileId { get; set; }

    public string Status { get; set; } = "Pending";

    public string EvidenceStorageKey { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAtUtc { get; set; }

    public string? ReviewedByUserId { get; set; }

    public string Notes { get; set; } = string.Empty;
}
