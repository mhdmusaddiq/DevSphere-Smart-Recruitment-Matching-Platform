namespace DevSphere.Application.DTOs.Candidates;

public class WorkExperienceDto
{
    public Guid Id { get; set; }
    public Guid CandidateProfileId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class EducationRecordDto
{
    public Guid Id { get; set; }
    public Guid CandidateProfileId { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class CertificationRecordDto
{
    public Guid Id { get; set; }
    public Guid CandidateProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public DateOnly IssuedOn { get; set; }
    public DateOnly? ExpiresOn { get; set; }
    public string CredentialUrl { get; set; } = string.Empty;
}

public class ProjectRecordDto
{
    public Guid Id { get; set; }
    public Guid CandidateProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
}

public class LanguageCapabilityDto
{
    public Guid Id { get; set; }
    public Guid CandidateProfileId { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Proficiency { get; set; } = string.Empty;
}
