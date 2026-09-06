using DevSphere.Application.DTOs.Employers;

namespace DevSphere.Application.Interfaces;

public interface IEmployerArchitectureService
{
    Task<CompanyVerificationDto> SubmitVerificationAsync(CompanyVerificationDto request, CancellationToken cancellationToken = default);

    Task<VacancyRequirementDto> AddRequirementAsync(VacancyRequirementDto request, CancellationToken cancellationToken = default);

    Task<RequiredSkillDto> AddRequiredSkillAsync(RequiredSkillDto request, CancellationToken cancellationToken = default);
}
