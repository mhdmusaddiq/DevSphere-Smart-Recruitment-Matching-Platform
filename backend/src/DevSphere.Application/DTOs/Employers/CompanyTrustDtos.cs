namespace DevSphere.Application.DTOs.Employers;

public class CompanyProfileDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Website { get; set; }

    public string? Location { get; set; }

    public string MembershipStatus { get; set; } = string.Empty;

    public string VerificationStatus { get; set; } = string.Empty;
}

public class CreateCompanyProfileRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Website { get; set; }

    public string? Location { get; set; }
}

public class UpdateCompanyProfileRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Website { get; set; }

    public string? Location { get; set; }
}

public class SubmitCompanyVerificationRequest
{
    public string EvidenceStorageKey { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}

public class CompanyVerificationResultDto
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; }
}
