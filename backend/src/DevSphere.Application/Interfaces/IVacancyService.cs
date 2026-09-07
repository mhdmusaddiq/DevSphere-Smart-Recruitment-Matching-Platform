using DevSphere.Application.DTOs.Profile;

namespace DevSphere.Application.Interfaces;

public interface IVacancyService
{
    Task<IEnumerable<VacancyDto>> GetOpenVacanciesAsync();

    Task<VacancyDto?> GetByIdAsync(Guid vacancyId);

    Task<VacancyDto> CreateAsync(
        string employerId,
        VacancyDto vacancy);

    Task<VacancyDto> UpdateAsync(
        string employerId,
        Guid vacancyId,
        VacancyDto vacancy);

    Task<VacancyDto> CloseAsync(
        string employerId,
        Guid vacancyId);
}