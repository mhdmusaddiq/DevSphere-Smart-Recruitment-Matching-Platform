using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Infrastructure.Data;

namespace DevSphere.Infrastructure.Repositories;

public class EmployerArchitectureRepository
{
    private readonly DevSphereDbContext _context;

    public EmployerArchitectureRepository(DevSphereDbContext context)
    {
        _context = context;
    }

    public async Task AddVerificationAsync(CompanyVerification verification, CancellationToken cancellationToken = default)
    {
        await _context.CompanyVerifications.AddAsync(verification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRequirementAsync(VacancyRequirement requirement, CancellationToken cancellationToken = default)
    {
        await _context.VacancyRequirements.AddAsync(requirement, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRequiredSkillAsync(RequiredSkill skill, CancellationToken cancellationToken = default)
    {
        await _context.RequiredSkills.AddAsync(skill, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
