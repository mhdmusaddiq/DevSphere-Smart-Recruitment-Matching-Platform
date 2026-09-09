using DevSphere.Application.DTOs.Profile;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Services.Profile;

public class VacancyService : IVacancyService
{
    private readonly VacancyRepository _repository;
    private readonly IVacancyPolicyService? _policyService;
    private readonly DevSphereDbContext? _context;

    public VacancyService(
        VacancyRepository repository,
        IVacancyPolicyService? policyService = null,
        DevSphereDbContext? context = null)
    {
        _repository = repository;
        _policyService = policyService;
        _context = context;
    }

    public async Task<IEnumerable<VacancyDto>> GetOpenVacanciesAsync(
        string? query,
        string? location,
        int page,
        int pageSize)
    {
        var vacancies = (await _repository.GetOpenAsync(
            query,
            location,
            page,
            pageSize)).ToList();

        return await MapManyAsync(vacancies);
    }

    public async Task<IEnumerable<VacancyDto>> GetMineAsync(
        string employerId)
    {
        ValidateEmployerId(employerId);

        var vacancies = (await _repository
            .GetMineAsync(employerId))
            .ToList();

        return await MapManyAsync(vacancies);
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

        var skills = await _repository
            .GetRequiredSkillsAsync(vacancy.Id);

        return MapToDto(vacancy, skills);
    }

    public async Task<VacancyDto> CreateAsync(
        string employerId,
        VacancyDto request)
    {
        ValidateEmployerId(employerId);
        ValidateRequest(request);

        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            EmployerId = employerId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Location = request.Location,
            MinExperienceMonths = request.MinExperienceMonths,
            MaxExperienceMonths = request.MaxExperienceMonths,
            RequiredExperienceMonths =
                request.MinExperienceMonths,
            RequiredEducation = request.RequiredEducation,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            ClosingDateUtc = request.ClosingDateUtc,

            LifecycleStatus =
                VacancyLifecycleStatus.Draft,

            IsOpen = false,
            CreatedAt = DateTime.UtcNow
        };

        var skills = CreateRequiredSkills(
            vacancy.Id,
            request.RequiredSkills);

        await _repository.AddAsync(
            vacancy,
            skills);

        return MapToDto(
            vacancy,
            skills);
    }

    public async Task<VacancyDto> UpdateAsync(
        string employerId,
        Guid vacancyId,
        VacancyDto request)
    {
        ValidateEmployerId(employerId);

        var vacancy = await GetOwnedVacancyAsync(
            employerId,
            vacancyId,
            "update");

        if (vacancy.LifecycleStatus ==
            VacancyLifecycleStatus.Closed)
        {
            throw new InvalidOperationException(
                "Closed vacancies cannot be updated.");
        }

        ValidateRequest(request);

        var isMaterialChange =
            await IsMaterialChangeAsync(
                vacancy,
                request);

        if (vacancy.LifecycleStatus ==
                VacancyLifecycleStatus.Published &&
            isMaterialChange)
        {
            if (await _repository.HasApplicationsAsync(
                vacancy.Id))
            {
                throw new InvalidOperationException(
                    "Material vacancy fields are locked after the first application.");
            }

            if (_policyService != null)
            {
                await _policyService
                    .EnsureMaterialRevisionForEditAsync(
                        employerId,
                        vacancy.Id);
            }
        }

        vacancy.Title = request.Title.Trim();
        vacancy.Description = request.Description;
        vacancy.Location = request.Location;
        vacancy.MinExperienceMonths =
            request.MinExperienceMonths;
        vacancy.MaxExperienceMonths =
            request.MaxExperienceMonths;
        vacancy.RequiredExperienceMonths =
            request.MinExperienceMonths;
        vacancy.RequiredEducation =
            request.RequiredEducation;
        vacancy.SalaryMin = request.SalaryMin;
        vacancy.SalaryMax = request.SalaryMax;
        vacancy.ClosingDateUtc =
            request.ClosingDateUtc;
        vacancy.UpdatedAt = DateTime.UtcNow;

        var skills = CreateRequiredSkills(
            vacancy.Id,
            request.RequiredSkills);

        await _repository.UpdateAsync(
            vacancy,
            skills);

        return MapToDto(
            vacancy,
            skills);
    }

    public async Task<VacancyDto> PublishAsync(
        string employerId,
        Guid vacancyId)
    {
        ValidateEmployerId(employerId);

        var vacancy = await GetOwnedVacancyAsync(
            employerId,
            vacancyId,
            "publish");

        if (vacancy.LifecycleStatus ==
            VacancyLifecycleStatus.Published)
        {
            throw new InvalidOperationException(
                "Vacancy is already published.");
        }

        if (vacancy.LifecycleStatus ==
            VacancyLifecycleStatus.Closed)
        {
            throw new InvalidOperationException(
                "Closed vacancies cannot be published.");
        }

        if (vacancy.ClosingDateUtc.HasValue &&
            vacancy.ClosingDateUtc.Value <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "A vacancy with an expired closing date cannot be published.");
        }

        await EnsureCanPublishAsync(employerId, vacancy);

        vacancy.LifecycleStatus =
            VacancyLifecycleStatus.Published;

        vacancy.IsOpen = true;
        vacancy.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(vacancy);

        var skills = await _repository
            .GetRequiredSkillsAsync(vacancy.Id);

        return MapToDto(
            vacancy,
            skills);
    }

    private async Task EnsureCanPublishAsync(
        string employerId,
        Vacancy vacancy)
    {
        if (_context == null || _policyService == null)
        {
            throw new InvalidOperationException(
                "Vacancy publication prerequisites cannot be verified.");
        }

        var employer = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == employerId);

        if (employer == null ||
            !employer.IsActive ||
            !employer.EmailConfirmed)
        {
            throw new InvalidOperationException(
                "An active, email-verified employer account is required to publish.");
        }

        var companyIds = await _context.CompanyMemberships
            .AsNoTracking()
            .Where(x =>
                x.EmployerUserId == employerId &&
                x.Status == CompanyMembershipStatus.Verified)
            .Select(x => x.CompanyId)
            .ToListAsync();

        var verificationRows = await _context.CompanyVerifications
            .AsNoTracking()
            .Where(x =>
                x.CompanyId.HasValue &&
                companyIds.Contains(x.CompanyId.Value))
            .OrderByDescending(x => x.SubmittedAtUtc)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();

        var hasVerifiedCompany = verificationRows
            .GroupBy(x => x.CompanyId!.Value)
            .Select(x => x.First())
            .Any(x => string.Equals(
                x.Status,
                CompanyVerificationStatus.Verified.ToString(),
                StringComparison.OrdinalIgnoreCase));

        if (!hasVerifiedCompany)
        {
            throw new InvalidOperationException(
                "A verified, unsuspended company and verified membership are required to publish.");
        }

        var currentPolicy = await _policyService
            .GetCurrentRevisionAsync(employerId, vacancy.Id);

        var hasPolicyInputs =
            await _context.VacancyRequirements.AnyAsync(x =>
                x.MatchingPolicyRevisionId == currentPolicy.Id &&
                x.IsActive) ||
            await _context.RequiredSkills.AnyAsync(x =>
                x.VacancyId == vacancy.Id);

        if (!hasPolicyInputs)
        {
            throw new InvalidOperationException(
                "A valid vacancy matching policy is required to publish.");
        }
    }

    public async Task<VacancyDto> CloseAsync(
        string employerId,
        Guid vacancyId)
    {
        ValidateEmployerId(employerId);

        var vacancy = await GetOwnedVacancyAsync(
            employerId,
            vacancyId,
            "close");

        if (vacancy.LifecycleStatus ==
            VacancyLifecycleStatus.Closed)
        {
            throw new InvalidOperationException(
                "Vacancy is already closed.");
        }

        if (vacancy.LifecycleStatus !=
            VacancyLifecycleStatus.Published)
        {
            throw new InvalidOperationException(
                "Only published vacancies can be closed.");
        }

        vacancy.LifecycleStatus =
            VacancyLifecycleStatus.Closed;

        vacancy.IsOpen = false;
        vacancy.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(vacancy);

        var skills = await _repository
            .GetRequiredSkillsAsync(vacancy.Id);

        return MapToDto(
            vacancy,
            skills);
    }

    private async Task<Vacancy> GetOwnedVacancyAsync(
        string employerId,
        Guid vacancyId,
        string action)
    {
        var vacancy = await _repository
            .GetByIdAsync(vacancyId);

        if (vacancy == null)
        {
            throw new KeyNotFoundException(
                "Vacancy not found.");
        }

        if (vacancy.EmployerId != employerId)
        {
            throw new UnauthorizedAccessException(
                $"You cannot {action} this vacancy.");
        }

        return vacancy;
    }

    private async Task<List<VacancyDto>> MapManyAsync(
        IReadOnlyCollection<Vacancy> vacancies)
    {
        if (vacancies.Count == 0)
        {
            return new List<VacancyDto>();
        }

        var skillsByVacancy = await _repository
            .GetRequiredSkillsAsync(
                vacancies.Select(x => x.Id));

        return vacancies
            .Select(vacancy =>
                MapToDto(
                    vacancy,
                    skillsByVacancy.TryGetValue(
                        vacancy.Id,
                        out var skills)
                        ? skills
                        : Array.Empty<RequiredSkill>()))
            .ToList();
    }

    private static List<RequiredSkill> CreateRequiredSkills(
        Guid vacancyId,
        IEnumerable<VacancyRequiredSkillDto> skills)
    {
        return skills
            .Select(skill => new RequiredSkill
            {
                Id = Guid.NewGuid(),
                VacancyId = vacancyId,
                Name = skill.Name.Trim(),
                Weight = skill.Weight,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();
    }

    private async Task<bool> IsMaterialChangeAsync(
        Vacancy vacancy,
        VacancyDto request)
    {
        if (vacancy.Location != request.Location ||
            vacancy.MinExperienceMonths !=
                request.MinExperienceMonths ||
            vacancy.MaxExperienceMonths !=
                request.MaxExperienceMonths ||
            vacancy.RequiredEducation !=
                request.RequiredEducation)
        {
            return true;
        }

        var existingSkills =
            await _repository
                .GetRequiredSkillsAsync(
                    vacancy.Id);

        var existing =
            existingSkills
                .Select(x => new
                {
                    Name = x.Name
                        .Trim()
                        .ToLowerInvariant(),
                    x.Weight
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Weight)
                .ToList();

        var requested =
            request.RequiredSkills
                .Select(x => new
                {
                    Name = x.Name
                        .Trim()
                        .ToLowerInvariant(),
                    x.Weight
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Weight)
                .ToList();

        if (existing.Count != requested.Count)
        {
            return true;
        }

        for (var index = 0;
             index < existing.Count;
             index++)
        {
            if (existing[index].Name !=
                    requested[index].Name ||
                existing[index].Weight !=
                    requested[index].Weight)
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateEmployerId(
        string employerId)
    {
        if (string.IsNullOrWhiteSpace(employerId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated employer is required.");
        }
    }

    private static void ValidateRequest(
        VacancyDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException(
                "Vacancy title is required.");
        }

        if (request.MinExperienceMonths < 0)
        {
            throw new ArgumentException(
                "Minimum experience cannot be negative.");
        }

        if (request.MaxExperienceMonths.HasValue &&
            request.MaxExperienceMonths.Value <
            request.MinExperienceMonths)
        {
            throw new ArgumentException(
                "Maximum experience must be greater than or equal to minimum experience.");
        }

        if (request.SalaryMin.HasValue &&
            request.SalaryMin.Value < 0)
        {
            throw new ArgumentException(
                "Minimum salary cannot be negative.");
        }

        if (request.SalaryMax.HasValue &&
            request.SalaryMax.Value < 0)
        {
            throw new ArgumentException(
                "Maximum salary cannot be negative.");
        }

        if (request.SalaryMin.HasValue &&
            request.SalaryMax.HasValue &&
            request.SalaryMax.Value <
            request.SalaryMin.Value)
        {
            throw new ArgumentException(
                "Maximum salary must be greater than or equal to minimum salary.");
        }

        if (request.ClosingDateUtc.HasValue &&
            request.ClosingDateUtc.Value <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "Closing date must be in the future.");
        }

        if (request.RequiredSkills == null ||
            request.RequiredSkills.Count == 0)
        {
            throw new ArgumentException(
                "At least one required skill is required.");
        }

        if (request.RequiredSkills.Any(
            x => string.IsNullOrWhiteSpace(x.Name)))
        {
            throw new ArgumentException(
                "Required skill name cannot be blank.");
        }

        var duplicateSkills = request.RequiredSkills
            .Select(x => x.Name.Trim().ToLowerInvariant())
            .GroupBy(x => x)
            .Any(x => x.Count() > 1);

        if (duplicateSkills)
        {
            throw new ArgumentException(
                "Duplicate required skills are not allowed.");
        }

        if (request.RequiredSkills.Any(
            x => x.Weight <= 0))
        {
            throw new ArgumentException(
                "Required skill weight must be greater than zero.");
        }
    }

    private static VacancyDto MapToDto(
        Vacancy vacancy,
        IEnumerable<RequiredSkill> requiredSkills)
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

            LifecycleStatus =
                vacancy.LifecycleStatus.ToString(),

            IsOpen =
                vacancy.IsOpen,

            RequiredSkills = requiredSkills
                .OrderBy(x => x.Name)
                .Select(x => new VacancyRequiredSkillDto
                {
                    Name = x.Name,
                    Weight = x.Weight
                })
                .ToList()
        };
    }
}
