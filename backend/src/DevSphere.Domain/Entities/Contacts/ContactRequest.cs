namespace DevSphere.Domain.Entities.Contacts;

public class ContactRequest : BaseEntity
{
    public string EmployerId { get; set; } = string.Empty;

    public string CandidateId { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}