namespace DevSphere.Application.DTOs.Profile;

public class CandidateSkillDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class AddCandidateSkillDto
{
    public string Name { get; set; } = string.Empty;
}
