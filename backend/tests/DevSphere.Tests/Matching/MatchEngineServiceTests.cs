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
        Assert.Equal(15, result.EducationScore);
        Assert.Equal(10, result.LocationScore);
        Assert.Equal(100, result.TotalScore);

        Assert.Contains("c#", result.MatchedSkills);
        Assert.Contains("asp.net", result.MatchedSkills);
        Assert.Empty(result.MissingSkills);
    }

    [Fact]
    public async Task CalculateAsync_Should_ReturnMissingSkills()
    {
        var repository = new FakeMatchingRepository
        {
            RequiredSkills =
            [
                new RequiredSkill { Name = "c#" },
                new RequiredSkill { Name = "sql" },
                new RequiredSkill { Name = "angular" }
            ]
        };

        repository.Candidate.Skills =
        [
            new Skill { Name = "c#" }
        ];

        var engine = new MatchEngineService(repository);

        var result = await engine.CalculateAsync(
            "candidate-1",
            "vacancy-1");

        Assert.Contains("c#", result.MatchedSkills);
        Assert.Contains("sql", result.MissingSkills);
        Assert.Contains("angular", result.MissingSkills);
    }

    [Fact]
    public async Task CalculateAsync_Should_BeDeterministic_And_NormalizeSkills()
    {
        var repository = new FakeMatchingRepository
        {
            RequiredSkills =
            [
                new RequiredSkill { Name = "  SQL  " },
                new RequiredSkill { Name = "C#" },
                new RequiredSkill { Name = "sql" }
            ]
        };

        repository.Candidate.Skills =
        [
            new Skill { Name = " c# " },
            new Skill { Name = "SQL" }
        ];

        var engine = new MatchEngineService(repository);

        var first = await engine.CalculateAsync(
            "candidate-1",
            "vacancy-1");

        var second = await engine.CalculateAsync(
            "candidate-1",
            "vacancy-1");

        Assert.Equal(first.TotalScore, second.TotalScore);
        Assert.Equal(first.SkillsScore, second.SkillsScore);

        Assert.Equal(
            first.MatchedSkills,
            second.MatchedSkills);

        Assert.Equal(
            ["c#", "sql"],
            first.MatchedSkills);

        Assert.Empty(first.MissingSkills);
        Assert.Equal(50, first.SkillsScore);
    }

    [Fact]
    public async Task CalculateAsync_Should_UseStructuredSkillExperienceRequirement()
    {
        var repository = new FakeMatchingRepository
        {
            RequiredSkills =
            [
                new RequiredSkill
                {
                    Name = "c#",
                    MinimumExperienceMonths = 24
                }
            ]
        };

        repository.Candidate.ExperienceMonths = 12;
        repository.Vacancy.RequiredExperienceMonths = 6;

        var engine = new MatchEngineService(repository);

        var result = await engine.CalculateAsync(
            "candidate-1",
            "vacancy-1");

        Assert.Equal(12.5, result.ExperienceScore);
    }
}

public class FakeMatchingRepository : MatchingRepository
{
    public CandidateProfile Candidate { get; set; }

    public Vacancy Vacancy { get; set; }

    public List<RequiredSkill> RequiredSkills { get; set; }

    public FakeMatchingRepository()
        : base(null!)
    {
        Candidate = new CandidateProfile
        {
            Id = Guid.NewGuid(),
            UserId = "candidate-1",
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
            Description = "Free text must not drive matching"
        };

        RequiredSkills =
        [
            new RequiredSkill { Name = "c#" },
            new RequiredSkill { Name = "asp.net" }
        ];
    }

    public override Task<CandidateProfile?> GetCandidateAsync(
        string userId)
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

    public override Task<List<RequiredSkill>> GetRequiredSkillsAsync(
        Guid vacancyId)
    {
        return Task.FromResult(
            RequiredSkills);
    }
}
