namespace DevSphere.Application.DTOs.Contacts;

public class ContactRequestDto
{
    public Guid Id { get; set; }

    public Guid JobApplicationId { get; set; }

    public string EmployerId { get; set; } = string.Empty;

    public string CandidateId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public Guid VacancyId { get; set; }

    public string VacancyTitle { get; set; } = string.Empty;

    public Guid? CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string CandidateDisplayName { get; set; } = string.Empty;

    public string EmployerDisplayName { get; set; } = string.Empty;

    public bool CanDiscloseContact { get; set; }

    public string? CandidateEmail { get; set; }
}
