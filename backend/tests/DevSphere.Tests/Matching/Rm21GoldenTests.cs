using DevSphere.Application.DTOs.Application;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Career;
using DevSphere.Domain.Entities.Taxonomy;
using DevSphere.Domain.Entities.Skills;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Matching;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Tests.Matching;

public class Rm21GoldenTests
{
    [Fact]
    public async Task Experience_Should_PreserveRawDecimal_And_RoundDisplayAwayFromZero()
    {
        var evidence = CreateEvidence();
        evidence.Candidate.ExperienceMonths = 1599;

        var policy = CreatePolicy(
            RequirementFamily.Experience,
            RequirementImportance.Medium,
            CreateRequirement(
                RequirementFamily.Experience,
                RequirementMode.Preferred,
                RequirementImportance.Medium,
                requiredMonths: 2000));

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            MatchAssessmentStatus.Calculated,
            result.AssessmentStatus);

        Assert.Equal(
            79.95m,
            result.RawCompatibility);

        Assert.Equal(
            80.0m,
            result.DisplayCompatibility);
    }

    [Fact]
    public async Task FamilyImportance_Should_Use_High3_And_Low1()
    {
        var evidence = CreateEvidence();
        evidence.Candidate.Skills.Add(
            new Skill
            {
                Name = "C#"
            });

        evidence.Candidate.AvailabilityStatus =
            "Available";

        var skillFamily = CreateFamily(
            RequirementFamily.Skill,
            RequirementImportance.High);

        var availabilityFamily = CreateFamily(
            RequirementFamily.Availability,
            RequirementImportance.Low);

        var skillRequirement = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Preferred,
            RequirementImportance.Medium,
            canonicalTargetKey: "c#");

        skillRequirement.FamilyPolicyId =
            skillFamily.Id;

        var availabilityRequirement =
            CreateRequirement(
                RequirementFamily.Availability,
                RequirementMode.Preferred,
                RequirementImportance.Medium,
                canonicalTargetKey:
                    "availabilitystatus",
                requiredValue:
                    "Unavailable");

        availabilityRequirement.FamilyPolicyId =
            availabilityFamily.Id;

        var policy = CreatePolicy(
            new[]
            {
                skillFamily,
                availabilityFamily
            },
            new[]
            {
                skillRequirement,
                availabilityRequirement
            });

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            75m,
            result.RawCompatibility);

        Assert.Equal(
            75m,
            result.DisplayCompatibility);
    }

    [Fact]
    public async Task InactiveOrUnscoredCriterion_Should_Not_DiluteDenominator()
    {
        var evidence = CreateEvidence();

        evidence.Candidate.Skills.Add(
            new Skill
            {
                Name = "C#"
            });

        var family = CreateFamily(
            RequirementFamily.Skill,
            RequirementImportance.Medium);

        var scored = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Preferred,
            RequirementImportance.High,
            canonicalTargetKey: "c#");

        scored.FamilyPolicyId = family.Id;

        var unscored = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Preferred,
            RequirementImportance.High,
            canonicalTargetKey: "java");

        unscored.FamilyPolicyId = family.Id;
        unscored.IsScored = false;

        var inactive = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Preferred,
            RequirementImportance.High,
            canonicalTargetKey: "python");

        inactive.FamilyPolicyId = family.Id;
        inactive.IsActive = false;

        var policy = CreatePolicy(
            new[] { family },
            new[]
            {
                scored,
                unscored,
                inactive
            });

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            100m,
            result.RawCompatibility);

        Assert.Equal(
            MatchAssessmentStatus.Calculated,
            result.AssessmentStatus);
    }

    [Fact]
    public async Task SkillMatching_Should_Not_Use_FuzzySubstringMatching()
    {
        var evidence = CreateEvidence();

        evidence.Candidate.Skills.Add(
            new Skill
            {
                Name = "JavaScript"
            });

        var policy = CreatePolicy(
            RequirementFamily.Skill,
            RequirementImportance.Medium,
            CreateRequirement(
                RequirementFamily.Skill,
                RequirementMode.Preferred,
                RequirementImportance.Medium,
                canonicalTargetKey: "java"));

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            0m,
            result.RawCompatibility);

        Assert.Equal(
            MatchCriterionState.NotDemonstrated,
            result.Families
                .Single()
                .Criteria
                .Single()
                .State);
    }

    [Fact]
    public async Task UnsupportedAvailabilityDimension_Should_BeIncomplete_NotGuessed()
    {
        var evidence = CreateEvidence();

        evidence.Candidate.AvailabilityStatus =
            "Available";

        var family = CreateFamily(
            RequirementFamily.Availability,
            RequirementImportance.Medium);

        var known = CreateRequirement(
            RequirementFamily.Availability,
            RequirementMode.Preferred,
            RequirementImportance.Medium,
            canonicalTargetKey:
                "availabilitystatus",
            requiredValue:
                "Available");

        known.FamilyPolicyId = family.Id;

        var unsupported = CreateRequirement(
            RequirementFamily.Availability,
            RequirementMode.Mandatory,
            RequirementImportance.Medium,
            canonicalTargetKey:
                "weekend",
            requiredValue:
                "Yes");

        unsupported.Description =
            "Weekend availability";

        unsupported.FamilyPolicyId =
            family.Id;

        var policy = CreatePolicy(
            new[] { family },
            new[]
            {
                known,
                unsupported
            });

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            MatchAssessmentStatus.Provisional,
            result.AssessmentStatus);

        Assert.Equal(
            MatchEligibilityStatus.IncompleteAssessment,
            result.Eligibility);

        Assert.Equal(
            100m,
            result.RawCompatibility);

        Assert.Equal(
            50m,
            result.Coverage);

        Assert.Contains(
            "weekend",
            result.MissingInputs);
    }

    [Fact]
    public async Task AnyOf_Should_ReduceMembers_To_One_MaxCriterion()
    {
        var evidence = CreateEvidence();

        evidence.Candidate.Skills.Add(
            new Skill
            {
                Name = "C#"
            });

        var family = CreateFamily(
            RequirementFamily.Skill,
            RequirementImportance.Medium);

        var set = CreateSet(
            family,
            AlternativeSetType.AnyOf,
            RequirementMode.Mandatory,
            RequirementImportance.High);

        var csharp = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Mandatory,
            RequirementImportance.High,
            canonicalTargetKey: "c#");

        csharp.FamilyPolicyId = family.Id;
        csharp.AlternativeSetId = set.Id;

        var java = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Mandatory,
            RequirementImportance.High,
            canonicalTargetKey: "java");

        java.FamilyPolicyId = family.Id;
        java.AlternativeSetId = set.Id;

        var policy = CreatePolicy(
            new[] { family },
            new[] { csharp, java },
            new[] { set });

        var result = await CalculateAsync(
            evidence,
            policy);

        var criteria =
            result.Families
                .Single()
                .Criteria;

        Assert.Single(criteria);

        Assert.Equal(
            100m,
            result.RawCompatibility);

        Assert.Equal(
            MatchCriterionState.Met,
            criteria.Single().State);

        Assert.Equal(
            set.Id,
            criteria.Single().AlternativeSetId);
    }

    [Fact]
    public async Task MinSatisfied_Should_Average_TopK_MemberScores()
    {
        var evidence = CreateEvidence();

        evidence.Candidate.Skills.Add(
            new Skill
            {
                Name = "C#"
            });

        var family = CreateFamily(
            RequirementFamily.Skill,
            RequirementImportance.Medium);

        var set = CreateSet(
            family,
            AlternativeSetType.MinSatisfied,
            RequirementMode.Mandatory,
            RequirementImportance.High,
            minimumSatisfiedCount: 2);

        var first = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Mandatory,
            RequirementImportance.High,
            canonicalTargetKey: "c#");

        var second = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Mandatory,
            RequirementImportance.High,
            canonicalTargetKey: "sql");

        var third = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Mandatory,
            RequirementImportance.High,
            canonicalTargetKey: "java");

        foreach (var requirement in
                 new[] { first, second, third })
        {
            requirement.FamilyPolicyId =
                family.Id;

            requirement.AlternativeSetId =
                set.Id;
        }

        var policy = CreatePolicy(
            new[] { family },
            new[]
            {
                first,
                second,
                third
            },
            new[] { set });

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            50m,
            result.RawCompatibility);

        Assert.Equal(
            MatchCriterionState.NotMet,
            result.Families
                .Single()
                .Criteria
                .Single()
                .State);

        Assert.Equal(
            MatchEligibilityStatus.DoesNotMeetBaseline,
            result.Eligibility);
    }

    [Fact]
    public async Task MalformedDuplicateFamilyPolicy_Should_ReturnCalculationFailure_WithNullScore()
    {
        var evidence = CreateEvidence();

        var first = CreateFamily(
            RequirementFamily.Skill,
            RequirementImportance.High);

        var duplicate = CreateFamily(
            RequirementFamily.Skill,
            RequirementImportance.Low);

        var requirement = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Preferred,
            RequirementImportance.Medium,
            canonicalTargetKey: "c#");

        requirement.FamilyPolicyId =
            first.Id;

        var policy = CreatePolicy(
            new[]
            {
                first,
                duplicate
            },
            new[]
            {
                requirement
            });

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            MatchAssessmentStatus.CalculationFailure,
            result.AssessmentStatus);

        Assert.Null(
            result.RawCompatibility);

        Assert.Null(
            result.DisplayCompatibility);
    }


    [Fact]
    public async Task SkillMatching_Should_Accept_ActiveCuratedAlias()
    {
        var evidence = CreateEvidence();

        var concept =
            new SkillConcept
            {
                Id = Guid.NewGuid(),
                Name = "C Sharp",
                NormalizedName = "csharp",
                IsActive = true
            };

        concept.Aliases.Add(
            new SkillAlias
            {
                Id = Guid.NewGuid(),
                SkillConceptId = concept.Id,
                Alias = ".NET",
                NormalizedAlias = "dotnet",
                IsActive = true,
                SkillConcept = concept
            });

        evidence.Candidate.Skills.Add(
            new Skill
            {
                Id = Guid.NewGuid(),
                Name = "C Sharp",
                SkillConceptId = concept.Id,
                SkillConcept = concept
            });

        var policy = CreatePolicy(
            RequirementFamily.Skill,
            RequirementImportance.Medium,
            CreateRequirement(
                RequirementFamily.Skill,
                RequirementMode.Preferred,
                RequirementImportance.Medium,
                canonicalTargetKey: "dotnet"));

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            100m,
            result.RawCompatibility);

        Assert.Equal(
            MatchCriterionState.Met,
            result.Families
                .Single()
                .Criteria
                .Single()
                .State);
    }

    [Fact]
    public async Task SkillMatching_Should_Reject_InactiveCuratedAlias()
    {
        var evidence = CreateEvidence();

        var concept =
            new SkillConcept
            {
                Id = Guid.NewGuid(),
                Name = "C Sharp",
                NormalizedName = "csharp",
                IsActive = true
            };

        concept.Aliases.Add(
            new SkillAlias
            {
                Id = Guid.NewGuid(),
                SkillConceptId = concept.Id,
                Alias = ".NET",
                NormalizedAlias = "dotnet",
                IsActive = false,
                SkillConcept = concept
            });

        evidence.Candidate.Skills.Add(
            new Skill
            {
                Id = Guid.NewGuid(),
                Name = "C Sharp",
                SkillConceptId = concept.Id,
                SkillConcept = concept
            });

        var policy = CreatePolicy(
            RequirementFamily.Skill,
            RequirementImportance.Medium,
            CreateRequirement(
                RequirementFamily.Skill,
                RequirementMode.Preferred,
                RequirementImportance.Medium,
                canonicalTargetKey: "dotnet"));

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            0m,
            result.RawCompatibility);

        Assert.Equal(
            MatchCriterionState.NotDemonstrated,
            result.Families
                .Single()
                .Criteria
                .Single()
                .State);
    }

    [Fact]
    public async Task CriterionImportance_Should_Use_High3_Medium2_Low1()
    {
        var evidence = CreateEvidence();

        evidence.Candidate.Skills.Add(
            new Skill
            {
                Name = "C#"
            });

        var family = CreateFamily(
            RequirementFamily.Skill,
            RequirementImportance.Medium);

        var high = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Preferred,
            RequirementImportance.High,
            canonicalTargetKey: "c#");

        var medium = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Preferred,
            RequirementImportance.Medium,
            canonicalTargetKey: "java");

        var low = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Preferred,
            RequirementImportance.Low,
            canonicalTargetKey: "python");

        foreach (var requirement in
                 new[] { high, medium, low })
        {
            requirement.FamilyPolicyId =
                family.Id;
        }

        var policy = CreatePolicy(
            new[] { family },
            new[] { high, medium, low });

        var result = await CalculateAsync(
            evidence,
            policy);

        // (100*3 + 0*2 + 0*1) / (3+2+1) = 50
        Assert.Equal(
            50m,
            result.RawCompatibility);
    }

    [Fact]
    public async Task NoActiveScoredFamily_Should_Return_NotCalculated_WithNullScore()
    {
        var evidence = CreateEvidence();

        var family = CreateFamily(
            RequirementFamily.Skill,
            RequirementImportance.High);

        family.IsActive = false;

        var requirement = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Preferred,
            RequirementImportance.High,
            canonicalTargetKey: "c#");

        requirement.FamilyPolicyId =
            family.Id;

        var policy = CreatePolicy(
            new[] { family },
            new[] { requirement });

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            MatchAssessmentStatus.NotCalculated,
            result.AssessmentStatus);

        Assert.Equal(
            MatchEligibilityStatus.IncompleteAssessment,
            result.Eligibility);

        Assert.Null(
            result.RawCompatibility);

        Assert.Null(
            result.DisplayCompatibility);
    }

    [Fact]
    public async Task RegulatoryGateFailure_Should_Take_Precedence_Over_IncompleteMandatoryInput()
    {
        var evidence = CreateEvidence();

        var skillFamily = CreateFamily(
            RequirementFamily.Skill,
            RequirementImportance.High);

        var questionFamily = CreateFamily(
            RequirementFamily.StructuredQuestion,
            RequirementImportance.High);

        var regulatory = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Mandatory,
            RequirementImportance.High,
            canonicalTargetKey: "regulated-skill");

        regulatory.FamilyPolicyId =
            skillFamily.Id;

        regulatory.IsRegulatoryGate =
            true;

        var question = CreateRequirement(
            RequirementFamily.StructuredQuestion,
            RequirementMode.Mandatory,
            RequirementImportance.High);

        question.FamilyPolicyId =
            questionFamily.Id;

        question.QuestionText =
            "Mandatory declaration";

        question.ExpectedAnswer =
            "yes";

        var policy = CreatePolicy(
            new[]
            {
                skillFamily,
                questionFamily
            },
            new[]
            {
                regulatory,
                question
            });

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            MatchEligibilityStatus.DoesNotMeetBaseline,
            result.Eligibility);

        Assert.Equal(
            "BlockedByRegulatoryGate",
            result.EligibilityReason);

        Assert.Equal(
            MatchAssessmentStatus.Provisional,
            result.AssessmentStatus);
    }

    [Fact]
    public async Task MandatoryNotMet_Should_Take_Precedence_Over_MandatoryIncomplete()
    {
        var evidence = CreateEvidence();

        var skillFamily = CreateFamily(
            RequirementFamily.Skill,
            RequirementImportance.High);

        var questionFamily = CreateFamily(
            RequirementFamily.StructuredQuestion,
            RequirementImportance.High);

        var missingSkill = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Mandatory,
            RequirementImportance.High,
            canonicalTargetKey: "java");

        missingSkill.FamilyPolicyId =
            skillFamily.Id;

        var question = CreateRequirement(
            RequirementFamily.StructuredQuestion,
            RequirementMode.Mandatory,
            RequirementImportance.High);

        question.FamilyPolicyId =
            questionFamily.Id;

        question.QuestionText =
            "Mandatory declaration";

        var policy = CreatePolicy(
            new[]
            {
                skillFamily,
                questionFamily
            },
            new[]
            {
                missingSkill,
                question
            });

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            MatchEligibilityStatus.DoesNotMeetBaseline,
            result.Eligibility);

        Assert.Equal(
            "MandatoryRequirementNotMet",
            result.EligibilityReason);
    }

    [Fact]
    public async Task MandatoryPendingVerification_Should_Return_PendingVerificationEligibility()
    {
        var evidence = CreateEvidence();

        evidence.Candidate.Skills.Add(
            new Skill
            {
                Name = "C#"
            });

        var requirement = CreateRequirement(
            RequirementFamily.Skill,
            RequirementMode.Mandatory,
            RequirementImportance.High,
            canonicalTargetKey: "c#");

        requirement.RequiresVerification =
            true;

        var policy = CreatePolicy(
            RequirementFamily.Skill,
            RequirementImportance.High,
            requirement);

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            MatchAssessmentStatus.Calculated,
            result.AssessmentStatus);

        Assert.Equal(
            MatchEligibilityStatus.PendingVerification,
            result.Eligibility);

        Assert.Equal(
            "MandatoryVerificationPending",
            result.EligibilityReason);

        Assert.Equal(
            MatchCriterionState.PendingVerification,
            result.Families
                .Single()
                .Criteria
                .Single()
                .State);
    }

    [Fact]
    public async Task RemoteWorkMode_Should_Not_Require_LocationMatch()
    {
        var evidence = CreateEvidence();

        evidence.Candidate.PreferredWorkMode =
            "Remote";

        evidence.Candidate.Location =
            "Jaffna";

        evidence.Candidate.PreferredLocation =
            "Jaffna";

        var policy = CreatePolicy(
            RequirementFamily.LocationWorkMode,
            RequirementImportance.Medium,
            CreateRequirement(
                RequirementFamily.LocationWorkMode,
                RequirementMode.Preferred,
                RequirementImportance.Medium,
                canonicalTargetKey: "workmode",
                requiredValue: "Remote"));

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            100m,
            result.RawCompatibility);

        Assert.Equal(
            MatchCriterionState.Met,
            result.Families
                .Single()
                .Criteria
                .Single()
                .State);
    }

    [Fact]
    public async Task LocationRequirement_Should_Accept_RelocationEvidence()
    {
        var evidence = CreateEvidence();

        evidence.Candidate.Location =
            "Jaffna";

        evidence.Candidate.PreferredLocation =
            "Jaffna";

        evidence.Candidate.WillingToRelocate =
            true;

        var policy = CreatePolicy(
            RequirementFamily.LocationWorkMode,
            RequirementImportance.Medium,
            CreateRequirement(
                RequirementFamily.LocationWorkMode,
                RequirementMode.Preferred,
                RequirementImportance.Medium,
                canonicalTargetKey: "location",
                requiredValue: "Colombo"));

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            100m,
            result.RawCompatibility);
    }

    [Fact]
    public async Task PreferredStructuredQuestion_Should_Not_Fabricate_Answer()
    {
        var evidence = CreateEvidence();

        var requirement = CreateRequirement(
            RequirementFamily.StructuredQuestion,
            RequirementMode.Preferred,
            RequirementImportance.Medium);

        requirement.QuestionText =
            "Do you agree to travel?";

        requirement.ExpectedAnswer =
            "yes";

        var policy = CreatePolicy(
            RequirementFamily.StructuredQuestion,
            RequirementImportance.Medium,
            requirement);

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            MatchAssessmentStatus.Calculated,
            result.AssessmentStatus);

        Assert.Equal(
            MatchEligibilityStatus.MeetsBaseline,
            result.Eligibility);

        Assert.Equal(
            0m,
            result.RawCompatibility);

        Assert.Equal(
            MatchCriterionState.NotDemonstrated,
            result.Families
                .Single()
                .Criteria
                .Single()
                .State);
    }

    [Fact]
    public async Task Education_Should_Match_AcceptedStructuredValue()
    {
        var baseEvidence = CreateEvidence();

        var evidence =
            new CandidateMatchingEvidence
            {
                Candidate = baseEvidence.Candidate,
                EducationRecords =
                    new List<EducationRecord>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Qualification = "BSc",
                            FieldOfStudy =
                                "Computer Science",
                            Institution =
                                "Example University",
                            StartDate =
                                new DateOnly(2020, 1, 1),
                            EndDate =
                                new DateOnly(2023, 1, 1)
                        }
                    }
            };

        var requirement = CreateRequirement(
            RequirementFamily.Education,
            RequirementMode.Preferred,
            RequirementImportance.Medium);

        requirement.AcceptedValuesJson =
            "[\"bsc\"]";

        var policy = CreatePolicy(
            RequirementFamily.Education,
            RequirementImportance.Medium,
            requirement);

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            100m,
            result.RawCompatibility);
    }

    [Fact]
    public async Task Certification_Should_Require_NonExpired_ExactMatch()
    {
        var baseEvidence = CreateEvidence();

        var evidence =
            new CandidateMatchingEvidence
            {
                Candidate = baseEvidence.Candidate,
                Certifications =
                    new List<CertificationRecord>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Azure Fundamentals",
                            Issuer = "Microsoft",
                            IssuedOn =
                                DateOnly.FromDateTime(
                                    DateTime.UtcNow.AddDays(-30)),
                            ExpiresOn =
                                DateOnly.FromDateTime(
                                    DateTime.UtcNow.AddDays(30))
                        }
                    }
            };

        var policy = CreatePolicy(
            RequirementFamily.Certification,
            RequirementImportance.Medium,
            CreateRequirement(
                RequirementFamily.Certification,
                RequirementMode.Preferred,
                RequirementImportance.Medium,
                canonicalTargetKey:
                    "Azure Fundamentals"));

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            100m,
            result.RawCompatibility);
    }

    [Fact]
    public async Task Licence_Should_Match_Valid_ExactRegistration()
    {
        var baseEvidence = CreateEvidence();

        var evidence =
            new CandidateMatchingEvidence
            {
                Candidate = baseEvidence.Candidate,
                Licences =
                    new List<LicenceRegistration>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Type =
                                "Engineering Council",
                            Class = "Professional",
                            Issuer = "Council",
                            Identifier = "LIC-001",
                            IssuedOn =
                                DateOnly.FromDateTime(
                                    DateTime.UtcNow.AddYears(-1)),
                            ExpiresOn =
                                DateOnly.FromDateTime(
                                    DateTime.UtcNow.AddYears(1)),
                            Status = "Valid",
                            VerificationStatus =
                                "Verified"
                        }
                    }
            };

        var policy = CreatePolicy(
            RequirementFamily.LicenceRegistration,
            RequirementImportance.Medium,
            CreateRequirement(
                RequirementFamily.LicenceRegistration,
                RequirementMode.Preferred,
                RequirementImportance.Medium,
                canonicalTargetKey:
                    "Engineering Council"));

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            100m,
            result.RawCompatibility);
    }

    [Fact]
    public async Task Language_Should_Accept_Proficiency_Above_RequiredLevel()
    {
        var baseEvidence = CreateEvidence();

        var evidence =
            new CandidateMatchingEvidence
            {
                Candidate = baseEvidence.Candidate,
                Languages =
                    new List<LanguageCapability>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Language = "English",
                            Proficiency = "B2"
                        }
                    }
            };

        var policy = CreatePolicy(
            RequirementFamily.Language,
            RequirementImportance.Medium,
            CreateRequirement(
                RequirementFamily.Language,
                RequirementMode.Preferred,
                RequirementImportance.Medium,
                canonicalTargetKey: "English",
                requiredValue: "B1"));

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            100m,
            result.RawCompatibility);
    }

    [Fact]
    public async Task ProjectPortfolio_Should_Score_Distinct_ProjectEvidence()
    {
        var baseEvidence = CreateEvidence();

        var evidence =
            new CandidateMatchingEvidence
            {
                Candidate = baseEvidence.Candidate,
                Projects =
                    new List<ProjectRecord>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Project One",
                            ProjectUrl =
                                "https://example.test/one"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Project Two",
                            ProjectUrl =
                                "https://example.test/two"
                        }
                    }
            };

        var policy = CreatePolicy(
            RequirementFamily.ProjectPortfolio,
            RequirementImportance.Medium,
            CreateRequirement(
                RequirementFamily.ProjectPortfolio,
                RequirementMode.Preferred,
                RequirementImportance.Medium,
                requiredValue: "2"));

        var result = await CalculateAsync(
            evidence,
            policy);

        Assert.Equal(
            100m,
            result.RawCompatibility);
    }
    private static async Task<MatchResultDto>
        CalculateAsync(
            CandidateMatchingEvidence evidence,
            VacancyMatchingPolicy policy)
    {
        var repository =
            new FakeMatchingRepository(
                evidence,
                policy);

        var service =
            new MatchEngineService(
                repository);

        return await service.CalculateAsync(
            evidence.Candidate.UserId,
            policy.Vacancy.Id.ToString());
    }

    private static CandidateMatchingEvidence
        CreateEvidence()
    {
        return new CandidateMatchingEvidence
        {
            Candidate =
                new CandidateProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = "candidate-1",
                    FullName = "Candidate One",
                    Skills = new List<Skill>()
                }
        };
    }

    private static VacancyMatchingPolicy CreatePolicy(
        RequirementFamily family,
        RequirementImportance familyImportance,
        VacancyRequirement requirement)
    {
        var familyPolicy =
            CreateFamily(
                family,
                familyImportance);

        requirement.FamilyPolicyId =
            familyPolicy.Id;

        return CreatePolicy(
            new[] { familyPolicy },
            new[] { requirement });
    }

    private static VacancyMatchingPolicy CreatePolicy(
        IReadOnlyList<FamilyPolicy> families,
        IReadOnlyList<VacancyRequirement> requirements,
        IReadOnlyList<AlternativeSet>? sets = null)
    {
        var vacancyId =
            Guid.NewGuid();

        var revision =
            new MatchingPolicyRevision
            {
                Id = Guid.NewGuid(),
                VacancyId = vacancyId,
                RevisionNumber = 1,
                IsCurrent = true
            };

        foreach (var family in families)
        {
            family.MatchingPolicyRevisionId =
                revision.Id;
        }

        foreach (var set in
                 sets ?? Array.Empty<AlternativeSet>())
        {
            set.MatchingPolicyRevisionId =
                revision.Id;
        }

        foreach (var requirement in requirements)
        {
            requirement.VacancyId =
                vacancyId;

            requirement.MatchingPolicyRevisionId =
                revision.Id;
        }

        return new VacancyMatchingPolicy
        {
            Vacancy =
                new Vacancy
                {
                    Id = vacancyId
                },
            Revision = revision,
            Families = families,
            Requirements = requirements,
            AlternativeSets =
                sets ??
                Array.Empty<AlternativeSet>()
        };
    }

    private static FamilyPolicy CreateFamily(
        RequirementFamily family,
        RequirementImportance importance)
    {
        return new FamilyPolicy
        {
            Id = Guid.NewGuid(),
            RequirementFamily = family,
            FamilyImportance = importance,
            IsActive = true,
            IsScored = true
        };
    }

    private static VacancyRequirement CreateRequirement(
        RequirementFamily family,
        RequirementMode mode,
        RequirementImportance importance,
        string? canonicalTargetKey = null,
        string? requiredValue = null,
        int? requiredMonths = null)
    {
        return new VacancyRequirement
        {
            Id = Guid.NewGuid(),
            RequirementFamily = family,
            Mode = mode,
            Importance = importance,
            IsActive = true,
            IsScored = true,
            IsMandatory =
                mode == RequirementMode.Mandatory,
            CanonicalTargetKey =
                canonicalTargetKey,
            RequiredValue =
                requiredValue,
            RequiredMonths =
                requiredMonths,
            Description = string.Empty
        };
    }

    private static AlternativeSet CreateSet(
        FamilyPolicy family,
        AlternativeSetType type,
        RequirementMode mode,
        RequirementImportance importance,
        int? minimumSatisfiedCount = null)
    {
        return new AlternativeSet
        {
            Id = Guid.NewGuid(),
            FamilyPolicyId = family.Id,
            SetType = type,
            MinimumSatisfiedCount =
                minimumSatisfiedCount,
            Mode = mode,
            Importance = importance,
            IsActive = true,
            IsScored = true
        };
    }

    private sealed class FakeMatchingRepository
        : MatchingRepository
    {
        private readonly CandidateMatchingEvidence
            _evidence;

        private readonly VacancyMatchingPolicy
            _policy;

        public FakeMatchingRepository(
            CandidateMatchingEvidence evidence,
            VacancyMatchingPolicy policy)
            : base(null!)
        {
            _evidence = evidence;
            _policy = policy;
        }

        public override Task<CandidateMatchingEvidence?>
            GetCandidateEvidenceAsync(
                string userId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                CandidateMatchingEvidence?>(
                    _evidence);
        }

        public override Task<VacancyMatchingPolicy?>
            GetVacancyPolicyAsync(
                string vacancyId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                VacancyMatchingPolicy?>(
                    _policy);
        }
    }
}
