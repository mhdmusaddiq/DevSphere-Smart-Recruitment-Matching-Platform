using DevSphere.Application.DTOs.Profile;

namespace DevSphere.Application.Interfaces;

public interface IVacancyService
{
    Task<IEnumerable<VacancyDto>> GetOpenVacanciesAsync(
        string? query,
        string? location,
        int page,
        int pageSize,
        string? skill = null,
        string? workMode = null,
        string? employmentType = null);

    Task<IEnumerable<VacancyDto>> GetBestMatchesAsync(
        string candidateId,
        string? query,
        string? location,
        int page,
        int pageSize,
        string? skill = null,
        string? workMode = null,
        string? employmentType = null);

    Task<IEnumerable<VacancyDto>> GetMineAsync(
        string employerId);

    Task<VacancyDto?> GetByIdAsync(
        Guid vacancyId);

    Task<VacancyDto> CreateAsync(
        string employerId,
        VacancyDto vacancy);

    Task<VacancyDto> UpdateAsync(
        string employerId,
        Guid vacancyId,
        VacancyDto vacancy);

    Task<VacancyDto> PublishAsync(
        string employerId,
        Guid vacancyId);

    Task<VacancyDto> CloseAsync(
        string employerId,
        Guid vacancyId);
}
