namespace DevSphere.Domain.Entities.Employers;

public class EmployerProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;
}