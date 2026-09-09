using DevSphere.Application.DTOs.Application;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Skills;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Matching;
using DevSphere.Infrastructure.Repositories;
using Xunit;

namespace DevSphere.Tests.Matching;

public class MatchEngineServiceTests
{
    [Fact]
    public async Task CalculateAsync_Should_ReturnCalculatedFullSkillMatch()
    {
        var repository = new FakeMatchingRepository();
        var engine = new MatchEngineService(repository);

        var result = await engine.CalculateAsync(
            "candidate-1",
            repository.Policy.Vacancy.Id.ToString());

        Assert.Equal(
            MatchAssessmentStatus.Calculated,
            result.AssessmentStatus);

        Assert.Equal(
            MatchEligibilityStatus.MeetsBaseline,
            result.Eligibility);

        Assert.Equal(100m, result.RawCompatibility);
        Assert.Equal(100m, result.DisplayCompatibility);
        Assert.Equal(100m, result.Coverage);

        Assert.Contains("c#", result.MatchedSkills);
        Assert.Empty(result.MissingSkills);
    }

    [Fact]
    public async Task CalculateAsync_Should_ApplyCriterionImportanceWeights()
    {
        var repository = new FakeMatchingRepository();

        var family =
            repository.Policy.Families.Single();

        repository.Policy = repository.CreatePolicy(
            familyImportance:
                RequirementImportance.Medium,
            requirements:
            [
                new VacancyRequirement
                {
                    Id = Guid.NewGuid(),
                    FamilyPolicyId = family.Id,
                    RequirementFamily =
                        RequirementFamily.Skill,
                    Mode = RequirementMode.Preferred,
                    Importance =
                        RequirementImportance.High,
                    IsActive = true,
                    IsScored = true,
                    CanonicalTargetKey = "c#",
                    DisplayOrder = 1
                },
                new VacancyRequirement
                {
                    Id = Guid.NewGuid(),
                    FamilyPolicyId = family.Id,
                    RequirementFamily =
                        RequirementFamily.Skill,
                    Mode = RequirementMode.Preferred,
                    Importance =
                        RequirementImportance.Low,
                    IsActive = true,
                    IsScored = true,
                    CanonicalTargetKey = "sql",
                    DisplayOrder = 2
                }
            ],
            existingFamilyId: family.Id);

        var engine = new MatchEngineService(repository);

        var result = await engine.CalculateAsync(
            "candidate-1",
            repository.Policy.Vacancy.Id.ToString());

        Assert.Equal(
            MatchAssessmentStatus.Calculated,
            result.AssessmentStatus);

        Assert.Equal(75m, result.RawCompatibility);
        Assert.Equal(75m, result.DisplayCompatibility);

        Assert.Contains("c#", result.MatchedSkills);
        Assert.Contains("sql", result.MissingSkills);
    }

    [Fact]
    public async Task CalculateAsync_Should_ReturnNotCalculated_When_NoPolicyExists()
    {
        var repository = new FakeMatchingRepository();

        repository.Policy =
            new VacancyMatchingPolicy
            {
                Vacancy = repository.Policy.Vacancy,
                Revision = null
            };

        var engine = new MatchEngineService(repository);

        var result = await engine.CalculateAsync(
            "candidate-1",
            repository.Policy.Vacancy.Id.ToString());

        Assert.Equal(
            MatchAssessmentStatus.NotCalculated,
            result.AssessmentStatus);

        Assert.Null(result.RawCompatibility);
        Assert.Null(result.DisplayCompatibility);
    }

    [Fact]
    public async Task CalculateAsync_Should_ReturnProvisional_When_MandatoryInputIsMissing()
    {
        var repository = new FakeMatchingRepository();

        var familyId = Guid.NewGuid();

        var family =
            new FamilyPolicy
            {
                Id = familyId,
                MatchingPolicyRevisionId =
                    repository.Policy.Revision!.Id,
                RequirementFamily =
                    RequirementFamily.StructuredQuestion,
                FamilyImportance =
                    RequirementImportance.High,
                IsActive = true,
                IsScored = true
            };

        var skillFamily =
            repository.Policy.Families.Single();

        var skillRequirement =
            repository.Policy.Requirements.Single();

        var question =
            new VacancyRequirement
            {
                Id = Guid.NewGuid(),
                MatchingPolicyRevisionId =
                    repository.Policy.Revision.Id,
                FamilyPolicyId = familyId,
                RequirementFamily =
                    RequirementFamily.StructuredQuestion,
                Mode = RequirementMode.Mandatory,
                Importance =
                    RequirementImportance.High,
                IsActive = true,
                IsScored = true,
                QuestionText =
                    "Do you hold the required declaration?",
                ExpectedAnswer = "yes",
                DisplayOrder = 1
            };

        repository.Policy =
            new VacancyMatchingPolicy
            {
                Vacancy = repository.Policy.Vacancy,
                Revision = repository.Policy.Revision,
                Families =
                [
                    skillFamily,
                    family
                ],
                Requirements =
                [
                    skillRequirement,
                    question
                ]
            };

        var engine = new MatchEngineService(repository);

        var result = await engine.CalculateAsync(
            "candidate-1",
            repository.Policy.Vacancy.Id.ToString());

        Assert.Equal(
            MatchAssessmentStatus.Provisional,
            result.AssessmentStatus);

        Assert.Equal(
            MatchEligibilityStatus.IncompleteAssessment,
            result.Eligibility);

        Assert.Equal(100m, result.RawCompatibility);
        Assert.Equal(50m, result.Coverage);

        Assert.Contains(
            result.MissingInputs,
            x => x.Contains(
                "declaration",
                StringComparison.OrdinalIgnoreCase));
    }
}

public class FakeMatchingRepository : MatchingRepository
{
    public CandidateMatchingEvidence Evidence { get; set; }

    public VacancyMatchingPolicy Policy { get; set; }

    public FakeMatchingRepository()
        : base(null!)
    {
        var candidate =
            new CandidateProfile
            {
                Id = Guid.NewGuid(),
                UserId = "candidate-1",
                Skills =
                [
                    new Skill
                    {
                        Id = Guid.NewGuid(),
                        Name = "c#"
                    }
                ]
            };

        Evidence =
            new CandidateMatchingEvidence
            {
                Candidate = candidate
            };

        Policy = CreatePolicy();
    }

    public VacancyMatchingPolicy CreatePolicy(
        RequirementImportance familyImportance =
            RequirementImportance.High,
        IReadOnlyList<VacancyRequirement>? requirements = null,
        Guid? existingFamilyId = null)
    {
        var vacancy =
            Policy?.Vacancy ??
            new Vacancy
            {
                Id = Guid.NewGuid(),
                Description =
                    "Free text must not drive matching"
            };

        var revision =
            Policy?.Revision ??
            new MatchingPolicyRevision
            {
                Id = Guid.NewGuid(),
                VacancyId = vacancy.Id,
                RevisionNumber = 1,
                IsCurrent = true
            };

        var familyId =
            existingFamilyId ??
            Guid.NewGuid();

        var family =
            new FamilyPolicy
            {
                Id = familyId,
                MatchingPolicyRevisionId =
                    revision.Id,
                RequirementFamily =
                    RequirementFamily.Skill,
                FamilyImportance =
                    familyImportance,
                IsActive = true,
                IsScored = true
            };

        var effectiveRequirements =
            requirements ??
            [
                new VacancyRequirement
                {
                    Id = Guid.NewGuid(),
                    MatchingPolicyRevisionId =
                        revision.Id,
                    FamilyPolicyId =
                        familyId,
                    RequirementFamily =
                        RequirementFamily.Skill,
                    Mode =
                        RequirementMode.Mandatory,
                    Importance =
                        RequirementImportance.High,
                    IsActive = true,
                    IsScored = true,
                    CanonicalTargetKey = "c#",
                    DisplayOrder = 1
                }
            ];

        foreach (var requirement in
                 effectiveRequirements)
        {
            requirement.MatchingPolicyRevisionId =
                revision.Id;

            requirement.FamilyPolicyId =
                familyId;
        }

        return new VacancyMatchingPolicy
        {
            Vacancy = vacancy,
            Revision = revision,
            Families = [family],
            Requirements = effectiveRequirements
        };
    }

    public override Task<CandidateMatchingEvidence?>
        GetCandidateEvidenceAsync(
            string userId,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult<CandidateMatchingEvidence?>(
            userId == "candidate-1"
                ? Evidence
                : null);
    }

    public override Task<VacancyMatchingPolicy?>
        GetVacancyPolicyAsync(
            string vacancyId,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult<VacancyMatchingPolicy?>(
            vacancyId ==
                Policy.Vacancy.Id.ToString()
                ? Policy
                : null);
    }
}
