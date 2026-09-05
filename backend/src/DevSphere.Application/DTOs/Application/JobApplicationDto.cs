namespace DevSphere.Application.DTOs.Application;

public class JobApplicationDto
{
    public string CandidateId { get; set; } = string.Empty;

    public Guid VacancyId { get; set; }

    public string Status { get; set; } = string.Empty;
}