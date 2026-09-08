namespace DevSphere.Domain.Entities.Career;

public class LicenceRegistration : BaseEntity
{
    public Guid CandidateProfileId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Class { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Identifier { get; set; } = string.Empty;

    public DateOnly IssuedOn { get; set; }

    public DateOnly? ExpiresOn { get; set; }

    public string Status { get; set; } = "Valid";

    public string VerificationStatus { get; set; } = "Pending";
}
