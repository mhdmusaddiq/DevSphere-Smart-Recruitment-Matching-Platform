namespace DevSphere.Application.DTOs.Profile;

public class VacancyDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int RequiredExperienceMonths { get; set; }

    public bool IsOpen { get; set; }

    public Guid Id { get; set; }
}