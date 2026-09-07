using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Vacancies;
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
        string userId,
        string vacancyId)
    {
        var candidate = await _repository
            .GetCandidateAsync(userId);

        var vacancy = await _repository
            .GetVacancyAsync(vacancyId);

        if (candidate is null || vacancy is null)
        {
            throw new KeyNotFoundException(
                "Candidate or vacancy not found.");
        }

        var structuredRequirements = await _repository
            .GetRequiredSkillsAsync(vacancy.Id);

        // =========================
        // Skills - 50%
        // =========================

        var requiredSkills = structuredRequirements
            .Select(x => Normalize(x.Name))
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        var candidateSkills = candidate.Skills
            .Select(x => Normalize(x.Name))
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        var matchedSkillList = requiredSkills
            .Where(candidateSkills.Contains)
            .ToList();

        var missingSkillList = requiredSkills
            .Where(x => !candidateSkills.Contains(x))
            .ToList();

        var skillsScore = requiredSkills.Count == 0
            ? 50d
            : ((double)matchedSkillList.Count / requiredSkills.Count) * 50d;

        // =========================
        // Experience - 25%
        // =========================

        var requiredExperienceMonths = ResolveRequiredExperienceMonths(
            vacancy.RequiredExperienceMonths,
            structuredRequirements);

        var experienceScore = requiredExperienceMonths <= 0
            ? 25d
            : Math.Min(
                (double)Math.Max(candidate.ExperienceMonths, 0) /
                requiredExperienceMonths,
                1d) * 25d;

        // =========================
        // Education - 15%
        // =========================
        //
        // The current Vacancy aggregate has no structured education
        // requirement. Do not infer education requirements from free-text
        // vacancy.Description. Until a structured requirement is available,
        // this family is neutral so it does not create a false mismatch.
        //
        var educationScore = 15d;

        // =========================
        // Location - 10%
        // =========================

        var candidateLocation = Normalize(candidate.Location);
        var vacancyLocation = Normalize(vacancy.Location);

        var locationScore =
            candidateLocation.Length > 0 &&
            vacancyLocation.Length > 0 &&
            candidateLocation == vacancyLocation
                ? 10d
                : 0d;

        return new MatchResultDto
        {
            MatchedSkills = matchedSkillList,
            MissingSkills = missingSkillList,
            SkillsScore = Round(skillsScore),
            ExperienceScore = Round(experienceScore),
            EducationScore = Round(educationScore),
            LocationScore = Round(locationScore),
            TotalScore = Round(
                skillsScore +
                experienceScore +
                educationScore +
                locationScore)
        };
    }

    private static int ResolveRequiredExperienceMonths(
        int vacancyRequiredExperienceMonths,
        IEnumerable<RequiredSkill> structuredRequirements)
    {
        var skillRequirement = structuredRequirements
            .Where(x => x.MinimumExperienceMonths.HasValue)
            .Select(x => Math.Max(
                x.MinimumExperienceMonths!.Value,
                0))
            .DefaultIfEmpty(0)
            .Max();

        return Math.Max(
            Math.Max(vacancyRequiredExperienceMonths, 0),
            skillRequirement);
    }

    private static string Normalize(string? value)
    {
        return string.Join(
                " ",
                (value ?? string.Empty)
                    .Normalize()
                    .Trim()
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
    }

    private static double Round(double value)
    {
        return Math.Round(
            value,
            2,
            MidpointRounding.AwayFromZero);
    }
}
