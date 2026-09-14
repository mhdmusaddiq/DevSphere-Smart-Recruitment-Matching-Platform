using DevSphere.Application.DTOs.Employers;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Employers;

public class EmployerArchitectureService : IEmployerArchitectureService
{
    private readonly EmployerArchitectureRepository _repository;

    public EmployerArchitectureService(EmployerArchitectureRepository repository)
    {
        _repository = repository;
    }

    public async Task<CompanyVerificationDto> SubmitVerificationAsync(CompanyVerificationDto request, CancellationToken cancellationToken = default)
    {
        var entity = new CompanyVerification
        {
            Id = Guid.NewGuid(),
            EmployerProfileId = request.EmployerProfileId,
            Status = "Pending",
            EvidenceStorageKey = request.EvidenceStorageKey,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            SubmittedAtUtc = DateTime.UtcNow
        };

        await _repository.AddVerificationAsync(entity, cancellationToken);
        request.Id = entity.Id;
        request.Status = entity.Status;
        return request;
    }

    public async Task<VacancyRequirementDto> AddRequirementAsync(VacancyRequirementDto request, CancellationToken cancellationToken = default)
    {
        var entity = new VacancyRequirement
        {
            Id = Guid.NewGuid(),
            VacancyId = request.VacancyId,
            Description = request.Description,
            IsMandatory = request.IsMandatory,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddRequirementAsync(entity, cancellationToken);
        request.Id = entity.Id;
        return request;
    }

    public async Task<RequiredSkillDto> AddRequiredSkillAsync(RequiredSkillDto request, CancellationToken cancellationToken = default)
    {
        var entity = new RequiredSkill
        {
            Id = Guid.NewGuid(),
            VacancyId = request.VacancyId,
            Name = request.Name,
            MinimumExperienceMonths = request.MinimumExperienceMonths,
            Weight = request.Weight,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddRequiredSkillAsync(entity, cancellationToken);
        request.Id = entity.Id;
        return request;
    }
}
