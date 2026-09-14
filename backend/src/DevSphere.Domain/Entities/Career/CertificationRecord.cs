namespace DevSphere.Domain.Entities.Career;

public class CertificationRecord : BaseEntity
{
    public Guid CandidateProfileId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public DateOnly IssuedOn { get; set; }

    public DateOnly? ExpiresOn { get; set; }

    public string CredentialUrl { get; set; } = string.Empty;
}
