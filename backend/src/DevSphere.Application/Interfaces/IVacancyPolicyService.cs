using DevSphere.Application.DTOs.Employers;

namespace DevSphere.Application.Interfaces;

public interface IVacancyPolicyService
{
    Task EnsureMaterialRevisionForEditAsync(
        string employerUserId,
        Guid vacancyId);

    Task<MatchingPolicyRevisionDto> GetCurrentRevisionAsync(
        string employerUserId,
        Guid vacancyId);

    Task<FamilyPolicyDto> AddFamilyPolicyAsync(
        string employerUserId,
        Guid vacancyId,
        FamilyPolicyRequest request);

    Task<VacancyRequirementPolicyDto> AddRequirementAsync(
        string employerUserId,
        Guid vacancyId,
        VacancyRequirementRequest request);

    Task<AlternativeSetDto> AddAlternativeSetAsync(
        string employerUserId,
        Guid vacancyId,
        AlternativeSetRequest request);
}

