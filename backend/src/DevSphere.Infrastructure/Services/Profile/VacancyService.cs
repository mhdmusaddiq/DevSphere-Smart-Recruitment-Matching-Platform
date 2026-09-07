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

    public async Task<IEnumerable<VacancyDto>> GetOpenVacanciesAsync(
    string? query,
    string? location,
    int page,
    int pageSize)
{
    var vacancies = await _repository
        .GetOpenAsync(
            query,
            location,
            page,
            pageSize);

    return vacancies
        .Select(MapToDto)
        .ToList();

    }

    public async Task<VacancyDto?> GetByIdAsync(
        Guid vacancyId)
    {
        var vacancy = await _repository
            .GetByIdAsync(vacancyId);

        if (vacancy == null)
        {
            return null;
        }

        return MapToDto(vacancy);
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

            MinExperienceMonths =
                request.MinExperienceMonths,

            MaxExperienceMonths =
                request.MaxExperienceMonths,

            RequiredExperienceMonths =
                request.MinExperienceMonths,

            RequiredEducation =
                request.RequiredEducation,

            SalaryMin =
                request.SalaryMin,

            SalaryMax =
                request.SalaryMax,

            ClosingDateUtc =
                request.ClosingDateUtc,

            IsOpen = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(vacancy);

        return MapToDto(vacancy);
    }

    public async Task<VacancyDto> UpdateAsync(
        string employerId,
        Guid vacancyId,
        VacancyDto request)
    {
        var vacancy = await _repository
            .GetByIdAsync(vacancyId);

        if (vacancy == null)
        {
            throw new Exception(
                "Vacancy not found.");
        }

        if (vacancy.EmployerId != employerId)
        {
            throw new UnauthorizedAccessException(
                "You cannot update this vacancy.");
        }

        vacancy.Title =
            request.Title;

        vacancy.Description =
            request.Description;

        vacancy.Location =
            request.Location;

        vacancy.MinExperienceMonths =
            request.MinExperienceMonths;

        vacancy.MaxExperienceMonths =
            request.MaxExperienceMonths;

        vacancy.RequiredExperienceMonths =
            request.MinExperienceMonths;

        vacancy.RequiredEducation =
            request.RequiredEducation;

        vacancy.SalaryMin =
            request.SalaryMin;

        vacancy.SalaryMax =
            request.SalaryMax;

        vacancy.ClosingDateUtc =
            request.ClosingDateUtc;

        vacancy.UpdatedAt =
            DateTime.UtcNow;

        await _repository.UpdateAsync(vacancy);

        return MapToDto(vacancy);
    }

    public async Task<VacancyDto> CloseAsync(
        string employerId,
        Guid vacancyId)
    {
        var vacancy = await _repository
            .GetByIdAsync(vacancyId);

        if (vacancy == null)
        {
            throw new Exception(
                "Vacancy not found.");
        }

        if (vacancy.EmployerId != employerId)
        {
            throw new UnauthorizedAccessException(
                "You cannot close this vacancy.");
        }

        vacancy.IsOpen = false;
        vacancy.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(vacancy);

        return MapToDto(vacancy);
    }

    private static VacancyDto MapToDto(
        Vacancy vacancy)
    {
        return new VacancyDto
        {
            Id = vacancy.Id,
            Title = vacancy.Title,
            Description = vacancy.Description,
            Location = vacancy.Location,

            RequiredExperienceMonths =
                vacancy.RequiredExperienceMonths,

            MinExperienceMonths =
                vacancy.MinExperienceMonths,

            MaxExperienceMonths =
                vacancy.MaxExperienceMonths,

            RequiredEducation =
                vacancy.RequiredEducation,

            SalaryMin =
                vacancy.SalaryMin,

            SalaryMax =
                vacancy.SalaryMax,

            ClosingDateUtc =
                vacancy.ClosingDateUtc,

            IsOpen = vacancy.IsOpen
        };
    }
}