using DevSphere.Application.DTOs.Profile;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Profile;

public class VacancyService : IVacancyService
{
    private readonly VacancyRepository _repository;


    public VacancyService(
        VacancyRepository repository)
    {
        _repository = repository;
    }


    public async Task<IEnumerable<VacancyDto>> GetOpenVacanciesAsync()
    {
        var vacancies = await _repository
            .GetOpenAsync();


        return vacancies
            .Select(x => new VacancyDto
            {
                Title = x.Title,
                Description = x.Description,
                Location = x.Location,
                RequiredExperienceMonths = x.RequiredExperienceMonths,
                IsOpen = x.IsOpen
            })
            .ToList();
    }



    public async Task<VacancyDto> CreateAsync(
        string employerId,
        VacancyDto request)
    {
        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            EmployerId = employerId,
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            RequiredExperienceMonths = request.RequiredExperienceMonths,
            IsOpen = true,
            CreatedAt = DateTime.UtcNow
        };


        await _repository.AddAsync(vacancy);

        return new VacancyDto
        {
            Title = vacancy.Title,
            Description = vacancy.Description,
            Location = vacancy.Location,
            RequiredExperienceMonths = vacancy.RequiredExperienceMonths,
            IsOpen = vacancy.IsOpen
        };
    }
}