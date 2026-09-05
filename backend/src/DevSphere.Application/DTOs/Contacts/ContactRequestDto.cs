namespace DevSphere.Application.DTOs.Contacts;

public class ContactRequestDto
{
    public Guid Id { get; set; }

    public string EmployerId { get; set; } = string.Empty;

    public string CandidateId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}