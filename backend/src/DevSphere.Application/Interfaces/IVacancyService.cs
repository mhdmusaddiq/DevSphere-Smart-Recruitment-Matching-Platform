using DevSphere.Application.DTOs.Profile;

namespace DevSphere.Application.Interfaces;

public interface IVacancyService
{
    Task<IEnumerable<VacancyDto>> GetOpenVacanciesAsync();

    Task<VacancyDto> CreateAsync(
        string employerId,
        VacancyDto vacancy);
}