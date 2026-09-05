using DevSphere.Application.DTOs.Application;
using DevSphere.Infrastructure.Matching;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Entities.Skills;

using Xunit;

namespace DevSphere.Tests.Matching;

public class MatchEngineServiceTests
{
    [Fact]
    public async Task CalculateAsync_Should_ReturnFullMatchScore()
    {
        var repository = new FakeMatchingRepository();

        var engine = new MatchEngineService(repository);


        var result = await engine.CalculateAsync(
            "candidate-1",
            "vacancy-1");


        Assert.Equal(50, result.SkillsScore);
        Assert.Equal(25, result.ExperienceScore);
        Assert.Equal(10, result.LocationScore);

        Assert.Contains(
            "c#",
            result.MatchedSkills);

        Assert.Equal(
    100,
    result.TotalScore);
    }



    [Fact]
    public async Task CalculateAsync_Should_ReturnMissingSkills()
    {
        var repository = new FakeMatchingRepository();

        repository.Vacancy.RequiredSkills =
        [
            new Skill { Name = "c#" },
            new Skill { Name = "sql" },
            new Skill { Name = "angular" }
        ];


        repository.Candidate.Skills =
        [
            new Skill { Name = "c#" }
        ];


        var engine = new MatchEngineService(repository);


        var result = await engine.CalculateAsync(
            "candidate-1",
            "vacancy-1");


        Assert.Contains(
            "sql",
            result.MissingSkills);

        Assert.Contains(
            "angular",
            result.MissingSkills);
    }
}



public class FakeMatchingRepository : MatchingRepository
{
    public CandidateProfile Candidate { get; set; }

    public Vacancy Vacancy { get; set; }


    public FakeMatchingRepository()
        : base(null!)
    {
        Candidate = new CandidateProfile
        {
            Id = Guid.NewGuid(),
            Location = "Colombo",
            ExperienceMonths = 12,
            Education = "BSc",
            Skills =
            [
                new Skill { Name = "c#" },
                new Skill { Name = "asp.net" }
            ]
        };


        Vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            Location = "Colombo",
            RequiredExperienceMonths = 12,
            Description = "BSc required",

            RequiredSkills =
            [
                new Skill { Name = "c#" },
                new Skill { Name = "asp.net" }
            ]
        };
    }


    public override Task<CandidateProfile?> GetCandidateAsync(
        string candidateId)
    {
        return Task.FromResult<CandidateProfile?>(
            Candidate);
    }


    public override Task<Vacancy?> GetVacancyAsync(
        string vacancyId)
    {
        return Task.FromResult<Vacancy?>(
            Vacancy);
    }
}