namespace DevSphere.Domain.Entities.Employers;

public class CompanyProfile : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Website { get; set; }

    public string? Location { get; set; }
}
