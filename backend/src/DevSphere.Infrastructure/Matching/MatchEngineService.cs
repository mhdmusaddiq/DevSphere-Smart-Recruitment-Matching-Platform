using System.Text.Json;
using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Career;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Enums;
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
        var evidence =
            await _repository.GetCandidateEvidenceAsync(
                candidateId);

        if (evidence == null)
        {
            throw new KeyNotFoundException(
                "Candidate was not found.");
        }

        var policy =
            await _repository.GetVacancyPolicyAsync(
                vacancyId);

        if (policy == null)
        {
            throw new KeyNotFoundException(
                "Vacancy was not found.");
        }

        if (policy.Revision == null)
        {
            return new MatchResultDto
            {
                AssessmentStatus =
                    MatchAssessmentStatus.NotCalculated,
                Eligibility =
                    MatchEligibilityStatus.IncompleteAssessment,
                EligibilityReason =
                    "No professional matching policy is configured.",
                RawCompatibility = null,
                DisplayCompatibility = null,
                HighTierAggregate = null,
                MediumTierAggregate = null,
                Coverage = 0m
            };
        }

        try
        {
            ValidatePolicy(policy);

            return Evaluate(
                evidence,
                policy);
        }
        catch (Exception ex)
        {
            return new MatchResultDto
            {
                AssessmentStatus =
                    MatchAssessmentStatus.CalculationFailure,
                Eligibility =
                    MatchEligibilityStatus.IncompleteAssessment,
                EligibilityReason =
                    $"Matching calculation failed: {ex.Message}",
                RawCompatibility = null,
                DisplayCompatibility = null,
                HighTierAggregate = null,
                MediumTierAggregate = null,
                Coverage = 0m
            };
        }
    }

    private static MatchResultDto Evaluate(
        CandidateMatchingEvidence evidence,
        VacancyMatchingPolicy policy)
    {
        var familyResults =
            new List<MatchFamilyResultDto>();

        var scoredCriteria =
            new List<WeightedCriterion>();

        foreach (var familyPolicy in policy.Families
                     .Where(x => x.IsActive && x.IsScored)
                     .OrderBy(x => x.RequirementFamily)
                     .ThenBy(x => x.Id))
        {
            var requirements = policy.Requirements
                .Where(x =>
                    x.FamilyPolicyId == familyPolicy.Id &&
                    x.RequirementFamily ==
                        familyPolicy.RequirementFamily &&
                    x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToList();

            var alternativeSets = policy.AlternativeSets
                .Where(x =>
                    x.FamilyPolicyId == familyPolicy.Id &&
                    x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToList();

            var criterionResults =
                new List<WeightedCriterion>();

            foreach (var requirement in requirements
                         .Where(x =>
                             x.AlternativeSetId == null))
            {
                var evaluated =
                    EvaluateRequirement(
                        evidence,
                        requirement);

                criterionResults.Add(
                    new WeightedCriterion(
                        evaluated,
                        requirement.Importance,
                        requirement.IsScored));
            }

            foreach (var set in alternativeSets)
            {
                var members = requirements
                    .Where(x =>
                        x.AlternativeSetId == set.Id)
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Id)
                    .ToList();

                var memberResults = members
                    .Select(x =>
                        EvaluateRequirement(
                            evidence,
                            x))
                    .ToList();

                var reduced =
                    ReduceAlternativeSet(
                        set,
                        members,
                        memberResults,
                        familyPolicy.RequirementFamily);

                criterionResults.Add(
                    new WeightedCriterion(
                        reduced,
                        set.Importance,
                        set.IsScored));
            }

            var scored =
                criterionResults
                    .Where(x => x.IsScored)
                    .ToList();

            if (scored.Count == 0)
            {
                continue;
            }

            var assessable =
                scored
                    .Where(x => x.Result.Score.HasValue)
                    .ToList();

            decimal? rawFamilyScore = null;

            if (assessable.Count > 0)
            {
                var numerator = assessable.Sum(x =>
                    x.Result.Score!.Value *
                    ImportanceWeight(x.Importance));

                var denominator = assessable.Sum(x =>
                    ImportanceWeight(x.Importance));

                rawFamilyScore =
                    denominator == 0m
                        ? null
                        : numerator / denominator;
            }

            familyResults.Add(
                new MatchFamilyResultDto
                {
                    Family =
                        familyPolicy.RequirementFamily.ToString(),
                    Importance =
                        familyPolicy.FamilyImportance.ToString(),
                    RawScore = rawFamilyScore,
                    DisplayScore =
                        rawFamilyScore.HasValue
                            ? RoundDisplay(rawFamilyScore.Value)
                            : null,
                    Criteria = criterionResults
                        .Select(x => x.Result)
                        .ToList()
                });

            foreach (var criterion in scored)
            {
                scoredCriteria.Add(criterion);
            }
        }

        if (familyResults.Count == 0)
        {
            return new MatchResultDto
            {
                AssessmentStatus =
                    MatchAssessmentStatus.NotCalculated,
                Eligibility =
                    MatchEligibilityStatus.IncompleteAssessment,
                EligibilityReason =
                    "No active scored matching family is configured.",
                RawCompatibility = null,
                DisplayCompatibility = null,
                HighTierAggregate = null,
                MediumTierAggregate = null,
                Coverage = 0m,
                Families = familyResults
            };
        }

        var calculableFamilies =
            familyResults
                .Where(x => x.RawScore.HasValue)
                .Select(x => new
                {
                    Result = x,
                    Policy = policy.Families.Single(f =>
                        f.RequirementFamily.ToString() ==
                            x.Family)
                })
                .ToList();

        decimal? rawCompatibility = null;

        if (calculableFamilies.Count > 0)
        {
            var numerator =
                calculableFamilies.Sum(x =>
                    x.Result.RawScore!.Value *
                    ImportanceWeight(
                        x.Policy.FamilyImportance));

            var denominator =
                calculableFamilies.Sum(x =>
                    ImportanceWeight(
                        x.Policy.FamilyImportance));

            rawCompatibility =
                denominator == 0m
                    ? null
                    : numerator / denominator;
        }

        var totalScoredCriteria =
            scoredCriteria.Count;

        var coveredScoredCriteria =
            scoredCriteria.Count(x =>
                x.Result.Score.HasValue);

        var coverage =
            totalScoredCriteria == 0
                ? 0m
                : 100m *
                  coveredScoredCriteria /
                  totalScoredCriteria;

        var allResults =
            scoredCriteria
                .Select(x => x.Result)
                .ToList();

        var assessmentStatus =
            ResolveAssessmentStatus(
                rawCompatibility,
                allResults);

        var eligibility =
            ResolveEligibility(
                allResults);

        var matchedSkills =
            allResults
                .Where(x =>
                    x.Family ==
                        RequirementFamily.Skill.ToString() &&
                    x.State == MatchCriterionState.Met &&
                    !string.IsNullOrWhiteSpace(x.Label))
                .Select(x => Normalize(x.Label))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

        var missingSkills =
            allResults
                .Where(x =>
                    x.Family ==
                        RequirementFamily.Skill.ToString() &&
                    x.State is
                        MatchCriterionState.NotMet or
                        MatchCriterionState.NotDemonstrated or
                        MatchCriterionState.Incomplete)
                .Select(x => Normalize(x.Label))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

        var missingInputs =
            allResults
                .Where(x =>
                    x.State ==
                        MatchCriterionState.Incomplete)
                .Select(x => x.Label)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

        return new MatchResultDto
        {
            AssessmentStatus = assessmentStatus,
            Eligibility = eligibility.Status,
            EligibilityReason = eligibility.Reason,
            RawCompatibility = rawCompatibility,
            DisplayCompatibility =
                rawCompatibility.HasValue
                    ? RoundDisplay(rawCompatibility.Value)
                    : null,
            HighTierAggregate =
                TierAggregate(
                    scoredCriteria,
                    RequirementImportance.High),
            MediumTierAggregate =
                TierAggregate(
                    scoredCriteria,
                    RequirementImportance.Medium),
            Coverage =
                RoundDisplay(coverage),
            MatchedSkills = matchedSkills,
            MissingSkills = missingSkills,
            MissingInputs = missingInputs,
            Families = familyResults
        };
    }

    private static MatchCriterionResultDto EvaluateRequirement(
        CandidateMatchingEvidence evidence,
        VacancyRequirement requirement)
    {
        if (!requirement.IsActive ||
            requirement.Mode ==
                RequirementMode.Informational ||
            !requirement.IsScored)
        {
            return BuildResult(
                requirement,
                MatchCriterionState.NotApplicable,
                null);
        }

        return requirement.RequirementFamily switch
        {
            RequirementFamily.Skill =>
                EvaluateSkill(evidence, requirement),

            RequirementFamily.Experience =>
                EvaluateExperience(evidence, requirement),

            RequirementFamily.Education =>
                EvaluateEducation(evidence, requirement),

            RequirementFamily.Certification =>
                EvaluateCertification(evidence, requirement),

            RequirementFamily.LicenceRegistration =>
                EvaluateLicence(evidence, requirement),

            RequirementFamily.Language =>
                EvaluateLanguage(evidence, requirement),

            RequirementFamily.LocationWorkMode =>
                EvaluateLocationWorkMode(evidence, requirement),

            RequirementFamily.Availability =>
                EvaluateAvailability(evidence, requirement),

            RequirementFamily.ProjectPortfolio =>
                EvaluateProjectPortfolio(evidence, requirement),

            RequirementFamily.StructuredQuestion =>
                EvaluateStructuredQuestion(requirement),

            _ => throw new InvalidOperationException(
                $"Unsupported requirement family: {requirement.RequirementFamily}.")
        };
    }

    private static MatchCriterionResultDto EvaluateSkill(
        CandidateMatchingEvidence evidence,
        VacancyRequirement requirement)
    {
        var canonical =
            Normalize(requirement.CanonicalTargetKey);

        var byConcept =
            requirement.SkillConceptId.HasValue &&
            evidence.Candidate.Skills.Any(x =>
                x.SkillConceptId ==
                    requirement.SkillConceptId);

        var byCanonicalName =
            !string.IsNullOrWhiteSpace(canonical) &&
            evidence.Candidate.Skills.Any(x =>
                Normalize(
                    x.SkillConcept?.NormalizedName ??
                    x.SkillConcept?.Name ??
                    x.Name) == canonical);

        var byCuratedAlias =
            !string.IsNullOrWhiteSpace(canonical) &&
            evidence.Candidate.Skills.Any(x =>
                x.SkillConcept?.Aliases
                    .Any(alias =>
                        alias.IsActive &&
                        Normalize(
                            alias.NormalizedAlias ??
                            alias.Alias) == canonical)
                    == true);

        var met =
            byConcept ||
            byCanonicalName ||
            byCuratedAlias;

        return BuildBinaryEvidenceResult(
            requirement,
            met,
            canonical);
    }

    private static MatchCriterionResultDto EvaluateExperience(
        CandidateMatchingEvidence evidence,
        VacancyRequirement requirement)
    {
        var requiredMonths =
            requirement.RequiredMonths;

        if (!requiredMonths.HasValue ||
            requiredMonths.Value <= 0)
        {
            throw new InvalidOperationException(
                $"Experience requirement {requirement.Id} must define RequiredMonths > 0.");
        }

        var scope =
            Normalize(
                requirement.CanonicalTargetKey ??
                requirement.RequiredValue);

        var relevant = evidence.WorkExperiences
            .Where(x =>
                string.IsNullOrWhiteSpace(scope) ||
                Normalize(x.JobTitle) == scope)
            .ToList();

        decimal actualMonths;

        if (relevant.Count == 0 &&
            string.IsNullOrWhiteSpace(scope))
        {
            actualMonths =
                Math.Max(
                    0,
                    evidence.Candidate.ExperienceMonths);
        }
        else
        {
            actualMonths =
                CalculateUniqueMonths(relevant);
        }

        var score =
            Math.Min(
                actualMonths / requiredMonths.Value,
                1m) * 100m;

        return BuildNumericResult(
            requirement,
            score);
    }

    private static MatchCriterionResultDto EvaluateEducation(
        CandidateMatchingEvidence evidence,
        VacancyRequirement requirement)
    {
        var accepted =
            ReadAcceptedValues(requirement);

        if (accepted.Count == 0)
        {
            throw new InvalidOperationException(
                $"Education requirement {requirement.Id} has no accepted structured values.");
        }

        var met =
            evidence.EducationRecords.Any(x =>
                accepted.Contains(
                    Normalize(x.Qualification)) ||
                accepted.Contains(
                    Normalize(x.FieldOfStudy)));

        return BuildBinaryEvidenceResult(
            requirement,
            met);
    }

    private static MatchCriterionResultDto EvaluateCertification(
        CandidateMatchingEvidence evidence,
        VacancyRequirement requirement)
    {
        var target =
            Normalize(
                requirement.CanonicalTargetKey ??
                requirement.RequiredValue);

        if (string.IsNullOrWhiteSpace(target))
        {
            throw new InvalidOperationException(
                $"Certification requirement {requirement.Id} has no canonical target.");
        }

        var today =
            DateOnly.FromDateTime(DateTime.UtcNow);

        var matching =
            evidence.Certifications
                .Where(x => Normalize(x.Name) == target)
                .OrderByDescending(x => x.ExpiresOn)
                .ThenBy(x => x.Id)
                .FirstOrDefault();

        var valid =
            matching != null &&
            (!matching.ExpiresOn.HasValue ||
             matching.ExpiresOn.Value >= today);

        if (valid && requirement.RequiresVerification)
        {
            return BuildResult(
                requirement,
                MatchCriterionState.PendingVerification,
                100m);
        }

        return BuildBinaryEvidenceResult(
            requirement,
            valid,
            target);
    }

    private static MatchCriterionResultDto EvaluateLicence(
        CandidateMatchingEvidence evidence,
        VacancyRequirement requirement)
    {
        var accepted =
            ReadAcceptedValues(requirement);

        var target =
            Normalize(
                requirement.CanonicalTargetKey ??
                requirement.RequiredValue);

        var today =
            DateOnly.FromDateTime(DateTime.UtcNow);

        var matching =
            evidence.Licences
                .Where(x =>
                    (!string.IsNullOrWhiteSpace(target) &&
                     (Normalize(x.Type) == target ||
                      Normalize(x.Class) == target)) ||
                    accepted.Contains(Normalize(x.Type)) ||
                    accepted.Contains(Normalize(x.Class)))
                .OrderBy(x => x.Id)
                .FirstOrDefault();

        var valid =
            matching != null &&
            (!matching.ExpiresOn.HasValue ||
             matching.ExpiresOn.Value >= today) &&
            !Normalize(matching.Status)
                .Equals(
                    "expired",
                    StringComparison.Ordinal) &&
            !Normalize(matching.Status)
                .Equals(
                    "rejected",
                    StringComparison.Ordinal);

        if (!valid)
        {
            return BuildBinaryEvidenceResult(
                requirement,
                false,
                target);
        }

        if (requirement.RequiresVerification &&
            Normalize(matching!.VerificationStatus)
                != "verified")
        {
            return BuildResult(
                requirement,
                MatchCriterionState.PendingVerification,
                100m);
        }

        return BuildBinaryEvidenceResult(
            requirement,
            true,
            target);
    }

    private static MatchCriterionResultDto EvaluateLanguage(
        CandidateMatchingEvidence evidence,
        VacancyRequirement requirement)
    {
        var language =
            Normalize(requirement.CanonicalTargetKey);

        var requiredLevel =
            Normalize(requirement.RequiredValue);

        if (string.IsNullOrWhiteSpace(language))
        {
            throw new InvalidOperationException(
                $"Language requirement {requirement.Id} has no canonical language.");
        }

        var capability =
            evidence.Languages
                .Where(x =>
                    Normalize(x.Language) == language)
                .OrderByDescending(x =>
                    LanguageRank(
                        Normalize(x.Proficiency)))
                .ThenBy(x => x.Id)
                .FirstOrDefault();

        if (capability == null)
        {
            return BuildBinaryEvidenceResult(
                requirement,
                false,
                language);
        }

        if (string.IsNullOrWhiteSpace(requiredLevel))
        {
            return BuildBinaryEvidenceResult(
                requirement,
                true,
                language);
        }

        var candidateRank =
            LanguageRank(
                Normalize(capability.Proficiency));

        var requiredRank =
            LanguageRank(requiredLevel);

        if (candidateRank == 0 ||
            requiredRank == 0)
        {
            return BuildBinaryEvidenceResult(
                requirement,
                Normalize(capability.Proficiency) ==
                    requiredLevel,
                language);
        }

        return BuildBinaryEvidenceResult(
            requirement,
            candidateRank >= requiredRank,
            language);
    }

    private static MatchCriterionResultDto EvaluateLocationWorkMode(
        CandidateMatchingEvidence evidence,
        VacancyRequirement requirement)
    {
        var target =
            Normalize(
                requirement.CanonicalTargetKey);

        var required =
            Normalize(
                requirement.RequiredValue);

        if (target is "workmode" or "work-mode")
        {
            var candidateMode =
                Normalize(
                    evidence.Candidate.PreferredWorkMode);

            var met =
                candidateMode == required;

            return BuildBinaryEvidenceResult(
                requirement,
                met,
                required);
        }

        if (required == "remote" ||
            target == "remote")
        {
            var candidateMode =
                Normalize(
                    evidence.Candidate.PreferredWorkMode);

            return BuildBinaryEvidenceResult(
                requirement,
                candidateMode == "remote",
                "remote");
        }

        var requiredLocation =
            !string.IsNullOrWhiteSpace(required)
                ? required
                : Normalize(
                    policySafeLocation(
                        requirement));

        var preferred =
            Normalize(
                evidence.Candidate.PreferredLocation);

        var current =
            Normalize(
                evidence.Candidate.Location);

        var locationMet =
            !string.IsNullOrWhiteSpace(requiredLocation) &&
            (preferred == requiredLocation ||
             current == requiredLocation ||
             evidence.Candidate.WillingToRelocate);

        return BuildBinaryEvidenceResult(
            requirement,
            locationMet,
            requiredLocation);
    }

    private static MatchCriterionResultDto EvaluateAvailability(
        CandidateMatchingEvidence evidence,
        VacancyRequirement requirement)
    {
        var key =
            Normalize(
                requirement.CanonicalTargetKey);

        var required =
            Normalize(
                requirement.RequiredValue);

        if (key is "availabilitystatus" or
            "availability-status" or
            "status")
        {
            if (string.IsNullOrWhiteSpace(
                    evidence.Candidate.AvailabilityStatus))
            {
                return BuildMissingResult(
                    requirement);
            }

            return BuildBinaryEvidenceResult(
                requirement,
                Normalize(
                    evidence.Candidate.AvailabilityStatus) ==
                    required,
                required);
        }

        if (key is "employmenttype" or
            "employment-type")
        {
            if (string.IsNullOrWhiteSpace(
                    evidence.Candidate
                        .PreferredEmploymentType))
            {
                return BuildMissingResult(
                    requirement);
            }

            return BuildBinaryEvidenceResult(
                requirement,
                Normalize(
                    evidence.Candidate
                        .PreferredEmploymentType) ==
                    required,
                required);
        }

        if (key is "noticeperioddays" or
            "notice-period-days")
        {
            if (!int.TryParse(
                    requirement.RequiredValue,
                    out var maxDays))
            {
                throw new InvalidOperationException(
                    $"Availability requirement {requirement.Id} has invalid notice-period days.");
            }

            if (!evidence.Candidate
                    .NoticePeriodDays.HasValue)
            {
                return BuildMissingResult(
                    requirement);
            }

            return BuildBinaryEvidenceResult(
                requirement,
                evidence.Candidate
                    .NoticePeriodDays.Value <= maxDays,
                requirement.RequiredValue);
        }

        if (key is "availablefrom" or
            "available-from")
        {
            if (!DateOnly.TryParse(
                    requirement.RequiredValue,
                    out var requiredDate))
            {
                throw new InvalidOperationException(
                    $"Availability requirement {requirement.Id} has invalid available-from date.");
            }

            if (!evidence.Candidate
                    .AvailableFrom.HasValue)
            {
                return BuildMissingResult(
                    requirement);
            }

            return BuildBinaryEvidenceResult(
                requirement,
                evidence.Candidate
                    .AvailableFrom.Value <= requiredDate,
                requirement.RequiredValue);
        }

        // A legacy availability criterion with no canonical key may
        // continue to use AvailabilityStatus. Any named but unsupported
        // dimension (for example shift, weekend or travel) must not be
        // inferred from another candidate field.
        if (string.IsNullOrWhiteSpace(key))
        {
            if (string.IsNullOrWhiteSpace(
                    evidence.Candidate.AvailabilityStatus))
            {
                return BuildMissingResult(
                    requirement);
            }

            return BuildBinaryEvidenceResult(
                requirement,
                Normalize(
                    evidence.Candidate.AvailabilityStatus) ==
                    required,
                required);
        }

        return BuildMissingResult(
            requirement);
    }

    private static MatchCriterionResultDto EvaluateProjectPortfolio(
        CandidateMatchingEvidence evidence,
        VacancyRequirement requirement)
    {
        var requiredCount =
            TryGetPositiveInt(
                requirement.RequiredValue)
            ?? requirement.RequiredMonths;

        if (!requiredCount.HasValue ||
            requiredCount.Value <= 0)
        {
            throw new InvalidOperationException(
                $"Project requirement {requirement.Id} must define a positive required count.");
        }

        var scope =
            Normalize(
                requirement.CanonicalTargetKey);

        var matchingCount =
            evidence.Projects
                .Where(x =>
                    string.IsNullOrWhiteSpace(scope) ||
                    Normalize(x.Name) == scope)
                .Select(x =>
                    string.IsNullOrWhiteSpace(x.ProjectUrl)
                        ? Normalize(x.Name)
                        : Normalize(x.ProjectUrl))
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .Count();

        var score =
            Math.Min(
                (decimal)matchingCount /
                    requiredCount.Value,
                1m) * 100m;

        return BuildNumericResult(
            requirement,
            score);
    }

    private static MatchCriterionResultDto EvaluateStructuredQuestion(
        VacancyRequirement requirement)
    {
        // The live application model currently has no persisted
        // candidate structured-question response. Do not invent,
        // infer, or auto-score an answer.
        if (requirement.Mode ==
            RequirementMode.Mandatory)
        {
            return BuildMissingResult(
                requirement);
        }

        return BuildResult(
            requirement,
            MatchCriterionState.NotDemonstrated,
            0m);
    }

    private static MatchCriterionResultDto ReduceAlternativeSet(
        AlternativeSet set,
        IReadOnlyList<VacancyRequirement> members,
        IReadOnlyList<MatchCriterionResultDto> memberResults,
        RequirementFamily family)
    {
        if (members.Count == 0)
        {
            throw new InvalidOperationException(
                $"AlternativeSet {set.Id} has no members.");
        }

        var scores =
            memberResults
                .Where(x => x.Score.HasValue)
                .Select(x => x.Score!.Value)
                .OrderByDescending(x => x)
                .ToList();

        decimal? reducedScore;

        if (set.SetType ==
            AlternativeSetType.AnyOf)
        {
            reducedScore =
                scores.Count == 0
                    ? null
                    : scores.Max();
        }
        else if (set.SetType ==
                 AlternativeSetType.MinSatisfied)
        {
            var k =
                set.MinimumSatisfiedCount;

            if (!k.HasValue ||
                k.Value < 1 ||
                k.Value > members.Count)
            {
                throw new InvalidOperationException(
                    $"AlternativeSet {set.Id} has invalid MinimumSatisfiedCount.");
            }

            if (scores.Count < k.Value)
            {
                reducedScore = null;
            }
            else
            {
                var topK =
                    scores.Take(k.Value).Sum();

                reducedScore =
                    (topK /
                     (100m * k.Value)) *
                    100m;
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported AlternativeSet type: {set.SetType}.");
        }

        var state =
            ResolveAlternativeSetState(
                set,
                memberResults,
                reducedScore);

        return new MatchCriterionResultDto
        {
            RequirementId = null,
            AlternativeSetId = set.Id,
            Family = family.ToString(),
            Mode = set.Mode.ToString(),
            Importance = set.Importance.ToString(),
            State = state,
            Score = reducedScore,
            IsRegulatoryGate =
                members.Any(x =>
                    x.IsRegulatoryGate),
            Label =
                $"AlternativeSet:{set.Id}"
        };
    }

    private static MatchCriterionState ResolveAlternativeSetState(
        AlternativeSet set,
        IReadOnlyList<MatchCriterionResultDto> members,
        decimal? reducedScore)
    {
        if (set.SetType ==
            AlternativeSetType.AnyOf)
        {
            if (members.Any(x =>
                    x.State ==
                    MatchCriterionState.Met))
            {
                return MatchCriterionState.Met;
            }
        }
        else if (set.SetType ==
                 AlternativeSetType.MinSatisfied &&
                 reducedScore == 100m)
        {
            return MatchCriterionState.Met;
        }

        if (members.Any(x =>
                x.State ==
                MatchCriterionState.PendingVerification))
        {
            return MatchCriterionState.PendingVerification;
        }

        if (members.Any(x =>
                x.State ==
                MatchCriterionState.Incomplete))
        {
            return MatchCriterionState.Incomplete;
        }

        return set.Mode ==
               RequirementMode.Mandatory
            ? MatchCriterionState.NotMet
            : MatchCriterionState.NotDemonstrated;
    }

    private static void ValidatePolicy(
        VacancyMatchingPolicy policy)
    {
        if (policy.Revision == null)
        {
            return;
        }

        var duplicates =
            policy.Families
                .GroupBy(x =>
                    x.RequirementFamily)
                .Where(x => x.Count() != 1)
                .Select(x => x.Key)
                .ToList();

        if (duplicates.Count > 0)
        {
            throw new InvalidOperationException(
                "Policy contains duplicate FamilyPolicy rows.");
        }

        foreach (var requirement in
                 policy.Requirements.Where(x =>
                     x.IsActive))
        {
            if (!requirement.FamilyPolicyId.HasValue)
            {
                throw new InvalidOperationException(
                    $"Requirement {requirement.Id} has no FamilyPolicy.");
            }

            var family =
                policy.Families.SingleOrDefault(x =>
                    x.Id ==
                        requirement.FamilyPolicyId.Value);

            if (family == null ||
                family.RequirementFamily !=
                    requirement.RequirementFamily)
            {
                throw new InvalidOperationException(
                    $"Requirement {requirement.Id} has a conflicting FamilyPolicy.");
            }
        }

        foreach (var set in
                 policy.AlternativeSets.Where(x =>
                     x.IsActive))
        {
            var family =
                policy.Families.SingleOrDefault(x =>
                    x.Id == set.FamilyPolicyId);

            if (family == null)
            {
                throw new InvalidOperationException(
                    $"AlternativeSet {set.Id} has no FamilyPolicy.");
            }

            var members =
                policy.Requirements
                    .Where(x =>
                        x.AlternativeSetId == set.Id &&
                        x.IsActive)
                    .ToList();

            if (members.Count == 0)
            {
                throw new InvalidOperationException(
                    $"AlternativeSet {set.Id} has no active members.");
            }

            if (members.Any(x =>
                    x.FamilyPolicyId !=
                        set.FamilyPolicyId ||
                    x.RequirementFamily !=
                        family.RequirementFamily ||
                    x.Mode != set.Mode ||
                    x.Importance != set.Importance ||
                    x.IsScored != set.IsScored))
            {
                throw new InvalidOperationException(
                    $"AlternativeSet {set.Id} contains conflicting members.");
            }

            var duplicateTargets =
                members
                    .Select(x =>
                        Normalize(
                            x.CanonicalTargetKey ??
                            x.SkillConceptId?.ToString()))
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .GroupBy(
                        x => x,
                        StringComparer.Ordinal)
                    .Any(x => x.Count() > 1);

            if (duplicateTargets)
            {
                throw new InvalidOperationException(
                    $"AlternativeSet {set.Id} contains duplicate canonical targets.");
            }

            if (set.SetType ==
                    AlternativeSetType.MinSatisfied &&
                (!set.MinimumSatisfiedCount.HasValue ||
                 set.MinimumSatisfiedCount.Value < 1 ||
                 set.MinimumSatisfiedCount.Value >
                    members.Count))
            {
                throw new InvalidOperationException(
                    $"AlternativeSet {set.Id} has invalid MinimumSatisfiedCount.");
            }
        }
    }

    private static MatchAssessmentStatus ResolveAssessmentStatus(
        decimal? rawCompatibility,
        IReadOnlyList<MatchCriterionResultDto> results)
    {
        if (!rawCompatibility.HasValue)
        {
            return MatchAssessmentStatus.NotCalculated;
        }

        if (results.Any(x =>
                x.State ==
                    MatchCriterionState.Incomplete))
        {
            return MatchAssessmentStatus.Provisional;
        }

        return MatchAssessmentStatus.Calculated;
    }

    private static (
        MatchEligibilityStatus Status,
        string Reason)
        ResolveEligibility(
            IReadOnlyList<MatchCriterionResultDto> results)
    {
        var regulatoryFailure =
            results.FirstOrDefault(x =>
                x.IsRegulatoryGate &&
                x.State ==
                    MatchCriterionState.NotMet);

        if (regulatoryFailure != null)
        {
            return (
                MatchEligibilityStatus
                    .DoesNotMeetBaseline,
                "BlockedByRegulatoryGate");
        }

        var mandatory =
            results.Where(x =>
                    x.Mode ==
                        RequirementMode.Mandatory.ToString())
                .ToList();

        if (mandatory.Any(x =>
                x.State ==
                    MatchCriterionState.NotMet))
        {
            return (
                MatchEligibilityStatus
                    .DoesNotMeetBaseline,
                "MandatoryRequirementNotMet");
        }

        if (mandatory.Any(x =>
                x.State ==
                    MatchCriterionState.Incomplete))
        {
            return (
                MatchEligibilityStatus
                    .IncompleteAssessment,
                "MandatoryInputMissing");
        }

        if (mandatory.Any(x =>
                x.State ==
                    MatchCriterionState.PendingVerification))
        {
            return (
                MatchEligibilityStatus
                    .PendingVerification,
                "MandatoryVerificationPending");
        }

        return (
            MatchEligibilityStatus.MeetsBaseline,
            "AllMandatoryRequirementsSatisfied");
    }

    private static MatchCriterionResultDto
        BuildBinaryEvidenceResult(
            VacancyRequirement requirement,
            bool met,
            string? label = null)
    {
        if (met)
        {
            return BuildResult(
                requirement,
                requirement.RequiresVerification
                    ? MatchCriterionState
                        .PendingVerification
                    : MatchCriterionState.Met,
                100m,
                label);
        }

        return BuildResult(
            requirement,
            requirement.Mode ==
                RequirementMode.Mandatory
                ? MatchCriterionState.NotMet
                : MatchCriterionState.NotDemonstrated,
            0m,
            label);
    }

    private static MatchCriterionResultDto
        BuildNumericResult(
            VacancyRequirement requirement,
            decimal score)
    {
        score =
            Math.Clamp(
                score,
                0m,
                100m);

        var state =
            score >= 100m
                ? MatchCriterionState.Met
                : requirement.Mode ==
                    RequirementMode.Mandatory
                    ? MatchCriterionState.NotMet
                    : MatchCriterionState
                        .NotDemonstrated;

        return BuildResult(
            requirement,
            state,
            score);
    }

    private static MatchCriterionResultDto
        BuildMissingResult(
            VacancyRequirement requirement)
    {
        return BuildResult(
            requirement,
            requirement.Mode ==
                RequirementMode.Mandatory
                ? MatchCriterionState.Incomplete
                : MatchCriterionState.NotDemonstrated,
            requirement.Mode ==
                RequirementMode.Mandatory
                ? null
                : 0m);
    }

    private static MatchCriterionResultDto BuildResult(
        VacancyRequirement requirement,
        MatchCriterionState state,
        decimal? score,
        string? label = null)
    {
        return new MatchCriterionResultDto
        {
            RequirementId = requirement.Id,
            AlternativeSetId =
                requirement.AlternativeSetId,
            Family =
                requirement.RequirementFamily
                    .ToString(),
            Mode = requirement.Mode.ToString(),
            Importance =
                requirement.Importance.ToString(),
            State = state,
            Score = score,
            IsRegulatoryGate =
                requirement.IsRegulatoryGate,
            Label =
                !string.IsNullOrWhiteSpace(label)
                    ? label
                    : !string.IsNullOrWhiteSpace(
                        requirement.CanonicalTargetKey)
                        ? requirement.CanonicalTargetKey
                        : !string.IsNullOrWhiteSpace(
                            requirement.QuestionText)
                            ? requirement.QuestionText
                            : !string.IsNullOrWhiteSpace(
                                requirement.Description)
                                ? requirement.Description
                                : requirement.Id.ToString()
        };
    }

    private static decimal? TierAggregate(
        IReadOnlyList<WeightedCriterion> criteria,
        RequirementImportance importance)
    {
        var values =
            criteria
                .Where(x =>
                    x.Importance == importance &&
                    x.IsScored &&
                    x.Result.Score.HasValue)
                .Select(x =>
                    x.Result.Score!.Value)
                .ToList();

        if (values.Count == 0)
        {
            return null;
        }

        return values.Average();
    }

    private static int ImportanceWeight(
        RequirementImportance importance)
    {
        return importance switch
        {
            RequirementImportance.High => 3,
            RequirementImportance.Medium => 2,
            RequirementImportance.Low => 1,
            _ => throw new InvalidOperationException(
                $"Unsupported importance: {importance}.")
        };
    }

    private static decimal RoundDisplay(
        decimal value)
    {
        return decimal.Round(
            value,
            1,
            MidpointRounding.AwayFromZero);
    }

    private static HashSet<string> ReadAcceptedValues(
        VacancyRequirement requirement)
    {
        var result =
            new HashSet<string>(
                StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(
                requirement.AcceptedValuesJson))
        {
            try
            {
                var values =
                    JsonSerializer.Deserialize<List<string>>(
                        requirement.AcceptedValuesJson);

                if (values != null)
                {
                    foreach (var value in values)
                    {
                        var normalized =
                            Normalize(value);

                        if (!string.IsNullOrWhiteSpace(
                                normalized))
                        {
                            result.Add(normalized);
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Requirement {requirement.Id} has invalid AcceptedValuesJson.",
                    ex);
            }
        }

        var required =
            Normalize(requirement.RequiredValue);

        if (!string.IsNullOrWhiteSpace(required))
        {
            result.Add(required);
        }

        return result;
    }

    private static decimal CalculateUniqueMonths(
        IReadOnlyList<WorkExperience> experiences)
    {
        if (experiences.Count == 0)
        {
            return 0m;
        }

        var today =
            DateOnly.FromDateTime(DateTime.UtcNow);

        var intervals =
            experiences
                .Select(x => new
                {
                    Start = x.StartDate,
                    End = x.EndDate ?? today
                })
                .Where(x => x.End >= x.Start)
                .OrderBy(x => x.Start)
                .ThenBy(x => x.End)
                .ToList();

        if (intervals.Count == 0)
        {
            return 0m;
        }

        var totalDays = 0;
        var currentStart = intervals[0].Start;
        var currentEnd = intervals[0].End;

        foreach (var interval in intervals.Skip(1))
        {
            if (interval.Start <=
                currentEnd.AddDays(1))
            {
                if (interval.End > currentEnd)
                {
                    currentEnd = interval.End;
                }

                continue;
            }

            totalDays +=
                currentEnd.DayNumber -
                currentStart.DayNumber + 1;

            currentStart = interval.Start;
            currentEnd = interval.End;
        }

        totalDays +=
            currentEnd.DayNumber -
            currentStart.DayNumber + 1;

        return totalDays / 30.4375m;
    }

    private static int LanguageRank(
        string value)
    {
        return value switch
        {
            "a1" => 1,
            "beginner" => 1,
            "basic" => 1,

            "a2" => 2,
            "elementary" => 2,

            "b1" => 3,
            "intermediate" => 3,

            "b2" => 4,
            "upper-intermediate" => 4,

            "c1" => 5,
            "advanced" => 5,

            "c2" => 6,
            "fluent" => 6,
            "native" => 6,

            _ => 0
        };
    }

    private static int? TryGetPositiveInt(
        string? value)
    {
        return int.TryParse(
                   value,
                   out var parsed) &&
               parsed > 0
            ? parsed
            : null;
    }

    private static string policySafeLocation(
        VacancyRequirement requirement)
    {
        return requirement.RequiredValue ??
               string.Empty;
    }

    private static string Normalize(
        string? value)
    {
        return (value ?? string.Empty)
            .Trim()
            .ToLowerInvariant();
    }

    private sealed record WeightedCriterion(
        MatchCriterionResultDto Result,
        RequirementImportance Importance,
        bool IsScored);
}
