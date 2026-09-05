using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Matching;

public class MatchEngineService : IMatchEngine
{
    private readonly MatchingRepository _repository;


    public MatchEngineService(
        MatchingRepository repository)
    {
        _repository = repository;
    }


    public async Task<MatchResultDto> CalculateAsync(
        string candidateId,
        string vacancyId)
    {
        var candidate = await _repository
            .GetCandidateAsync(candidateId);

        var vacancy = await _repository
            .GetVacancyAsync(vacancyId);


        if (candidate == null || vacancy == null)
        {
            throw new Exception(
                "Candidate or vacancy not found.");
        }


        // =========================
        // Skills - 50%
        // =========================

        var requiredSkills = vacancy.RequiredSkills?
    .Select(x => x.Name?.Trim().ToLower() ?? "")
    .Where(x => !string.IsNullOrWhiteSpace(x))
    .ToList()
    ?? new List<string>();


        var candidateSkills = candidate.Skills?
            .Select(x => x.Name?.Trim().ToLower() ?? "")
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList()
            ?? new List<string>();

        var matchedSkills = requiredSkills
            .Intersect(candidateSkills)
            .Count();

        Console.WriteLine("Candidate Skills:");
        foreach (var skill in candidateSkills)
        {
            Console.WriteLine(skill);
        }

        Console.WriteLine("Required Skills:");
        foreach (var skill in requiredSkills)
        {
            Console.WriteLine(skill);
        }

        Console.WriteLine($"Matched Skills: {matchedSkills}");

        double skillsScore = 0;

        if (requiredSkills.Count > 0)
        {
            skillsScore =
                ((double)matchedSkills / requiredSkills.Count) * 50;
        }


        // =========================
        // Experience - 25%
        // =========================

        double experienceScore;

        if (vacancy.RequiredExperienceMonths == 0)
        {
            experienceScore = 25;
        }
        else
        {
            experienceScore =
                Math.Min(
                    (double)candidate.ExperienceMonths /
                    vacancy.RequiredExperienceMonths,
                    1
                ) * 25;
        }

        // =========================
        // Education - 15%
        // =========================

        double educationScore = 0;

        if (!string.IsNullOrWhiteSpace(candidate.Education) &&
            !string.IsNullOrWhiteSpace(vacancy.Description) &&
            vacancy.Description.Contains(
                candidate.Education,
                StringComparison.OrdinalIgnoreCase))
        {
            educationScore = 15;
        }

        // =========================
        // Location - 10%
        // =========================

        double locationScore = 0;

        if (!string.IsNullOrWhiteSpace(candidate.Location) &&
            !string.IsNullOrWhiteSpace(vacancy.Location) &&
            candidate.Location.Equals(
                vacancy.Location,
                StringComparison.OrdinalIgnoreCase))
        {
            locationScore = 10;
        }



        // =========================
        // Result
        // =========================

        Console.WriteLine($"Skills Score: {skillsScore}");
        Console.WriteLine($"Experience Score: {experienceScore}");
        Console.WriteLine($"Education Score: {educationScore}");
        Console.WriteLine($"Location Score: {locationScore}");

        return new MatchResultDto
        {
            SkillsScore = Math.Round(skillsScore, 2),

            ExperienceScore = Math.Round(
                experienceScore,
                2),

            EducationScore = Math.Round(
    educationScore,
    2),

            LocationScore = Math.Round(
    locationScore,
    2),

            TotalScore = Math.Round(
    skillsScore +
    experienceScore +
    educationScore +
    locationScore,
    2)
        };

    }
}