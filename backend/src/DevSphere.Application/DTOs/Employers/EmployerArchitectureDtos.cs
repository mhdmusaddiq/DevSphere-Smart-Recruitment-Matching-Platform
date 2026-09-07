namespace DevSphere.Application.DTOs.Employers;

public class CompanyVerificationDto
{
    public Guid Id { get; set; }

    public Guid EmployerProfileId { get; set; }

    public string Status { get; set; } = "Pending";

    public string EvidenceStorageKey { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}

public class VacancyRequirementDto
{
    public Guid Id { get; set; }

    public Guid VacancyId { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsMandatory { get; set; }
}

public class RequiredSkillDto
{
    public Guid Id { get; set; }

    public Guid VacancyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? MinimumExperienceMonths { get; set; }

    public int Weight { get; set; }
}