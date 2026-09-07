using DevSphere.Application.DTOs.Profile;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Services.Profile;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevSphere.Tests.Vacancies;

public class VacancyServiceTests
{
    private static DevSphereDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DevSphereDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DevSphereDbContext(options);
    }

    private static VacancyService CreateService(
        DevSphereDbContext context)
    {
        var repository = new VacancyRepository(context);

        return new VacancyService(repository);
    }

    private static VacancyDto CreateValidRequest()
    {
        return new VacancyDto
        {
            Title = "Junior .NET Developer",
            Description = "Build web APIs.",
            Location = "Jaffna",
            MinExperienceMonths = 12,
            MaxExperienceMonths = 36,
            RequiredEducation = "BSc or equivalent",
            SalaryMin = 100000,
            SalaryMax = 160000,
            ClosingDateUtc = DateTime.UtcNow.AddDays(30),

            RequiredSkills = new List<VacancyRequiredSkillDto>
            {
                new()
                {
                    Name = "C#",
                    Weight = 3
                },
                new()
                {
                    Name = "ASP.NET Core",
                    Weight = 3
                }
            }
        };
    }

    [Fact]
    public async Task Create_Should_Reject_Blank_Title()
    {
        using var context = CreateContext();

        var service = CreateService(context);

        var request = CreateValidRequest();

        request.Title = " ";

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                "employer-1",
                request));

        Assert.Equal(
            "Vacancy title is required.",
            exception.Message);
    }

    [Fact]
    public async Task Create_Should_Reject_Duplicate_Normalized_Skills()
    {
        using var context = CreateContext();

        var service = CreateService(context);

        var request = CreateValidRequest();

        request.RequiredSkills = new List<VacancyRequiredSkillDto>
        {
            new()
            {
                Name = "Angular",
                Weight = 2
            },
            new()
            {
                Name = " angular ",
                Weight = 3
            }
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                "employer-1",
                request));

        Assert.Equal(
            "Duplicate required skills are not allowed.",
            exception.Message);
    }

    [Fact]
    public async Task Create_Should_Reject_Invalid_Experience_Range()
    {
        using var context = CreateContext();

        var service = CreateService(context);

        var request = CreateValidRequest();

        request.MinExperienceMonths = 36;
        request.MaxExperienceMonths = 12;

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                "employer-1",
                request));

        Assert.Equal(
            "Maximum experience must be greater than or equal to minimum experience.",
            exception.Message);
    }

    [Fact]
    public async Task Create_Should_Reject_Invalid_Salary_Range()
    {
        using var context = CreateContext();

        var service = CreateService(context);

        var request = CreateValidRequest();

        request.SalaryMin = 200000;
        request.SalaryMax = 100000;

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                "employer-1",
                request));

        Assert.Equal(
            "Maximum salary must be greater than or equal to minimum salary.",
            exception.Message);
    }

    [Fact]
    public async Task Create_Should_Reject_Past_Closing_Date()
    {
        using var context = CreateContext();

        var service = CreateService(context);

        var request = CreateValidRequest();

        request.ClosingDateUtc =
            DateTime.UtcNow.AddDays(-1);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                "employer-1",
                request));

        Assert.Equal(
            "Closing date must be in the future.",
            exception.Message);
    }

    [Fact]
    public async Task Update_Should_Reject_Foreign_Employer()
    {
        using var context = CreateContext();

        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            EmployerId = "employer-owner",
            Title = "Backend Developer",
            Description = "Existing vacancy",
            Location = "Jaffna",
            MinExperienceMonths = 12,
            RequiredExperienceMonths = 12,
            IsOpen = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Vacancies.Add(vacancy);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var request = CreateValidRequest();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.UpdateAsync(
                "another-employer",
                vacancy.Id,
                request));
    }

    [Fact]
    public async Task Update_Should_Reject_Closed_Vacancy()
    {
        using var context = CreateContext();

        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            EmployerId = "employer-1",
            Title = "Backend Developer",
            Description = "Existing vacancy",
            Location = "Jaffna",
            MinExperienceMonths = 12,
            RequiredExperienceMonths = 12,
            IsOpen = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Vacancies.Add(vacancy);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var request = CreateValidRequest();

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(
                    "employer-1",
                    vacancy.Id,
                    request));

        Assert.Equal(
            "Closed vacancies cannot be updated.",
            exception.Message);
    }

    [Fact]
    public async Task Close_Should_Close_Own_Vacancy()
    {
        using var context = CreateContext();

        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            EmployerId = "employer-1",
            Title = "Backend Developer",
            Description = "Existing vacancy",
            Location = "Jaffna",
            MinExperienceMonths = 12,
            RequiredExperienceMonths = 12,
            IsOpen = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Vacancies.Add(vacancy);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.CloseAsync(
            "employer-1",
            vacancy.Id);

        Assert.False(result.IsOpen);

        var storedVacancy = await context.Vacancies
            .FindAsync(vacancy.Id);

        Assert.NotNull(storedVacancy);
        Assert.False(storedVacancy!.IsOpen);
    }

    [Fact]
    public async Task Close_Should_Reject_Already_Closed_Vacancy()
    {
        using var context = CreateContext();

        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            EmployerId = "employer-1",
            Title = "Backend Developer",
            Description = "Existing vacancy",
            Location = "Jaffna",
            MinExperienceMonths = 12,
            RequiredExperienceMonths = 12,
            IsOpen = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Vacancies.Add(vacancy);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CloseAsync(
                    "employer-1",
                    vacancy.Id));

        Assert.Equal(
            "Vacancy is already closed.",
            exception.Message);
    }

    [Fact]
    public async Task Search_Should_Filter_Open_Vacancies_By_Query_And_Location()
    {
        using var context = CreateContext();

        context.Vacancies.AddRange(
            new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerId = "employer-1",
                Title = "Junior .NET Developer",
                Description = "Build APIs",
                Location = "Jaffna",
                IsOpen = true,
                CreatedAt = DateTime.UtcNow
            },
            new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerId = "employer-2",
                Title = "Angular Developer",
                Description = "Frontend work",
                Location = "Colombo",
                IsOpen = true,
                CreatedAt = DateTime.UtcNow
            },
            new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerId = "employer-3",
                Title = "Senior .NET Developer",
                Description = "Closed vacancy",
                Location = "Jaffna",
                IsOpen = false,
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var results = await service
            .GetOpenVacanciesAsync(
                ".NET",
                "Jaffna",
                1,
                20);

        var list = results.ToList();

        Assert.Single(list);
        Assert.Equal(
            "Junior .NET Developer",
            list[0].Title);
        Assert.True(list[0].IsOpen);
    }

    [Fact]
    public async Task Search_Should_Apply_Paging()
    {
        using var context = CreateContext();

        context.Vacancies.AddRange(
            new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerId = "employer-1",
                Title = "Vacancy 1",
                Description = "Test",
                Location = "Jaffna",
                IsOpen = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-3)
            },
            new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerId = "employer-1",
                Title = "Vacancy 2",
                Description = "Test",
                Location = "Jaffna",
                IsOpen = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new Vacancy
            {
                Id = Guid.NewGuid(),
                EmployerId = "employer-1",
                Title = "Vacancy 3",
                Description = "Test",
                Location = "Jaffna",
                IsOpen = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var results = await service
            .GetOpenVacanciesAsync(
                null,
                null,
                2,
                1);

        var list = results.ToList();

        Assert.Single(list);

        Assert.Equal(
            "Vacancy 2",
            list[0].Title);
    }
}