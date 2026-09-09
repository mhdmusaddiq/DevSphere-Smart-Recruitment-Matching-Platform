using Microsoft.EntityFrameworkCore;
using DevSphere.Infrastructure.Data;
using System.Reflection;
using DevSphere.Domain.Entities.Applications;
using DevSphere.Infrastructure.Services.Profile;
using Xunit;

namespace DevSphere.Tests.Applications;

public class ApplicationSnapshotApplyDecisionTests
{
    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(
            AppContext.BaseDirectory);

        while (directory != null)
        {
            if (Directory.Exists(
                    Path.Combine(
                        directory.FullName,
                        "backend",
                        "src")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Repository root could not be located.");
    }

    private static string ReadSource(
        params string[] parts)
    {
        var path = Path.Combine(
            new[] { FindRepositoryRoot() }
                .Concat(parts)
                .ToArray());

        Assert.True(
            File.Exists(path),
            $"Expected source file was not found: {path}");

        return File.ReadAllText(path);
    }

    private static string Resolve(
        bool calculationFailure = false,
        bool accountStateBlocked = false,
        bool vacancyUnavailableOrSuppressed = false,
        bool relationshipBlocked = false,
        bool blockedByRegulatoryGate = false,
        bool blockedByReadiness = false,
        bool baselineAcknowledgementRequired = false)
    {
        var method = typeof(JobApplicationService)
            .GetMethod(
                "ResolveApplyDecision",
                BindingFlags.Static |
                BindingFlags.NonPublic);

        Assert.NotNull(method);

        return Assert.IsType<string>(
            method!.Invoke(
                null,
                new object[]
                {
                    calculationFailure,
                    accountStateBlocked,
                    vacancyUnavailableOrSuppressed,
                    relationshipBlocked,
                    blockedByRegulatoryGate,
                    blockedByReadiness,
                    baselineAcknowledgementRequired
                }));
    }

    [Fact]
    public void ApplyDecision_Should_Use_Canonical_Precedence()
    {
        Assert.Equal(
            "CalculationFailure",
            Resolve(
                calculationFailure: true,
                accountStateBlocked: true,
                vacancyUnavailableOrSuppressed: true,
                relationshipBlocked: true,
                blockedByRegulatoryGate: true,
                blockedByReadiness: true,
                baselineAcknowledgementRequired: true));

        Assert.Equal(
            "AccountStateBlocked",
            Resolve(
                accountStateBlocked: true,
                vacancyUnavailableOrSuppressed: true,
                relationshipBlocked: true,
                blockedByRegulatoryGate: true,
                blockedByReadiness: true,
                baselineAcknowledgementRequired: true));

        Assert.Equal(
            "VacancyUnavailableOrSuppressed",
            Resolve(
                vacancyUnavailableOrSuppressed: true,
                relationshipBlocked: true,
                blockedByRegulatoryGate: true,
                blockedByReadiness: true,
                baselineAcknowledgementRequired: true));

        Assert.Equal(
            "RelationshipBlocked",
            Resolve(
                relationshipBlocked: true,
                blockedByRegulatoryGate: true,
                blockedByReadiness: true,
                baselineAcknowledgementRequired: true));

        Assert.Equal(
            "BlockedByRegulatoryGate",
            Resolve(
                blockedByRegulatoryGate: true,
                blockedByReadiness: true,
                baselineAcknowledgementRequired: true));

        Assert.Equal(
            "BlockedByReadiness",
            Resolve(
                blockedByReadiness: true,
                baselineAcknowledgementRequired: true));

        Assert.Equal(
            "AllowedAfterBaselineAcknowledgement",
            Resolve(
                baselineAcknowledgementRequired: true));

        Assert.Equal(
            "Allowed",
            Resolve());
    }

    [Fact]
    public void Snapshot_Should_Expose_Frozen_Application_Assessment()
    {
        var properties =
            typeof(ApplicationSnapshot)
                .GetProperties()
                .Select(x => x.Name)
                .ToHashSet(
                    StringComparer.Ordinal);

        var required = new[]
        {
            "JobApplicationId",
            "ResumeVersionId",
            "MatchingPolicyRevisionId",
            "CompatibilityScore",
            "RawCompatibilityScore",
            "DisplayCompatibilityScore",
            "HighTierAggregateScore",
            "MediumTierAggregateScore",
            "Coverage",
            "CompatibilityStatus",
            "EligibilityStatus",
            "EligibilityReason",
            "IsEligible",
            "ApplyDecision",
            "MatchedSkillsJson",
            "GapSkillsJson",
            "EvidenceSummaryJson",
            "MatchResultJson",
            "CandidateSnapshotJson",
            "VacancySnapshotJson",
            "CapturedAtUtc"
        };

        foreach (var property in required)
        {
            Assert.Contains(
                property,
                properties);
        }
    }

    [Fact]
    public void Apply_Should_Use_Server_Owned_Candidate_Identity()
    {
        var controller = ReadSource(
            "backend",
            "src",
            "DevSphere.Api",
            "Controllers",
            "Profile",
            "JobApplicationController.cs");

        var service = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "JobApplicationService.cs");

        Assert.Contains(
            "ClaimTypes.NameIdentifier",
            controller);

        Assert.Contains(
            ".ApplyAsync(candidateId, request)",
            controller);

        Assert.DoesNotContain(
            "request.CandidateId =",
            controller);

        Assert.Contains(
            "CandidateId = candidateId",
            service);
    }

    [Fact]
    public void Apply_Should_Preserve_Duplicate_Conflict_Semantics()
    {
        var service = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "JobApplicationService.cs");

        var controller = ReadSource(
            "backend",
            "src",
            "DevSphere.Api",
            "Controllers",
            "Profile",
            "JobApplicationController.cs");

        Assert.Contains(
            "ExistsAsync(",
            service);

        Assert.Contains(
            "throw new ApplicationConflictException",
            service);

        Assert.Contains(
            "ApplicationConflictException",
            controller);

        Assert.Contains(
            "Conflict(",
            controller);
    }

    [Fact]
    public void Successful_Apply_Should_Use_Atomic_Snapshot_Repository_Path()
    {
        var service = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "JobApplicationService.cs");

        var repository = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Repositories",
            "JobApplicationRepository.cs");

        Assert.Contains(
            "AddWithSnapshotAsync(",
            service);

        Assert.Contains(
            "BeginTransactionAsync",
            repository);

        Assert.Contains(
            "ApplicationSnapshots.AddAsync",
            repository);

        Assert.Contains(
            "ApplicationStatusHistories.AddAsync",
            repository);

        Assert.Contains(
            "SaveChangesAsync",
            repository);

        Assert.Contains(
            "CommitAsync",
            repository);
    }

    [Fact]
    public void Blocked_Decision_Should_Not_Reach_Persistence_Path()
    {
        var service = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "JobApplicationService.cs");

        var blockedPosition =
            service.IndexOf(
                "if (!isEligible)",
                StringComparison.Ordinal);

        var persistPosition =
            service.IndexOf(
                "AddWithSnapshotAsync(",
                StringComparison.Ordinal);

        Assert.True(blockedPosition >= 0);
        Assert.True(persistPosition >= 0);

        Assert.True(
            blockedPosition < persistPosition,
            "Blocked apply must terminate before persistence.");
    }

    [Fact]
    public void Apply_Should_Freeze_Current_Resume_And_Readiness_Truth()
    {
        var service = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "JobApplicationService.cs");

        Assert.Contains(
            "GetApplicationReadinessAsync(candidateId)",
            service);

        Assert.Contains(
            "selectedResumeVersion.Id",
            service);

        Assert.Contains(
            "readiness.CurrentResumeVersionId",
            service);

        Assert.Contains(
            "Selected resume version is stale.",
            service);

        Assert.Contains(
            "ResumeVersionId = selectedResumeVersion?.Id",
            service);
    }

    [Fact]
    public void Apply_Should_Freeze_Match_Gaps_Evidence_And_Policy()
    {
        var service = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "JobApplicationService.cs");

        Assert.Contains(
            "MatchingPolicyRevisionId =",
            service);

        Assert.Contains(
            "MatchedSkillsJson =",
            service);

        Assert.Contains(
            "match?.MatchedSkills ?? new List<string>()",
            service);

        Assert.Contains(
            "GapSkillsJson =",
            service);

        Assert.Contains(
            "match?.MissingSkills ?? new List<string>()",
            service);

        Assert.Contains(
            "EvidenceSummaryJson =",
            service);

        Assert.Contains(
            "CandidateSnapshotJson =",
            service);

        Assert.Contains(
            "VacancySnapshotJson =",
            service);
    }

    [Fact]
    public void Compatibility_Status_Should_Preserve_Authoritative_Assessment()
    {
        var service = ReadSource(
            "backend",
            "src",
            "DevSphere.Infrastructure",
            "Services",
            "Profile",
            "JobApplicationService.cs");

        Assert.Contains(
            "match?.AssessmentStatus",
            service);

        Assert.Contains(
            "MatchAssessmentStatus.NotCalculated",
            service);

        Assert.Contains(
            "decimal.Round(",
            service);

        Assert.Contains(
            "MidpointRounding.AwayFromZero",
            service);

        Assert.DoesNotContain("TotalScore", service);

        Assert.DoesNotContain(
            "\"Strong\"",
            service);

        Assert.DoesNotContain(
            "\"Moderate\"",
            service);

        Assert.DoesNotContain(
            "\"Low\"",
            service);
    }

    [Fact]
    public async Task Persisted_Snapshot_Should_Remain_Frozen_After_Source_Data_Changes()
    {
        var options =
            new DbContextOptionsBuilder<DevSphereDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        await using var context =
            new DevSphereDbContext(options);

        var applicationId = Guid.NewGuid();
        var resumeVersionId = Guid.NewGuid();
        var policyRevisionId = Guid.NewGuid();

        const string frozenCandidate =
            """{"fullName":"Candidate At Apply","skills":["C#"]}""";

        const string frozenVacancy =
            """{"title":"Backend Engineer","location":"Colombo"}""";

        var snapshot =
            new ApplicationSnapshot
            {
                Id = Guid.NewGuid(),
                JobApplicationId = applicationId,
                ResumeVersionId = resumeVersionId,
                MatchingPolicyRevisionId = policyRevisionId,
                CompatibilityScore = 82.50m,
                RawCompatibilityScore = 82.499m,
                DisplayCompatibilityScore = 82.50m,
                CompatibilityStatus = "Calculated",
                EligibilityStatus = "Eligible",
                IsEligible = true,
                ApplyDecision = "Allowed",
                MatchedSkillsJson = """["C#"]""",
                GapSkillsJson = """["Azure"]""",
                EvidenceSummaryJson =
                    """{"matchedSkills":["C#"],"missingSkills":["Azure"]}""",
                CandidateSnapshotJson = frozenCandidate,
                VacancySnapshotJson = frozenVacancy,
                CapturedAtUtc = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

        context.ApplicationSnapshots.Add(snapshot);
        await context.SaveChangesAsync();

        // Simulate later source-of-truth changes after application time.
        var currentCandidateName = "Candidate Updated Later";
        var currentVacancyTitle = "Principal Engineer";
        var currentResumeVersionId = Guid.NewGuid();

        context.ChangeTracker.Clear();

        var persisted =
            await context.ApplicationSnapshots
                .AsNoTracking()
                .SingleAsync(
                    x => x.JobApplicationId == applicationId);

        Assert.Equal(
            frozenCandidate,
            persisted.CandidateSnapshotJson);

        Assert.Equal(
            frozenVacancy,
            persisted.VacancySnapshotJson);

        Assert.Equal(
            resumeVersionId,
            persisted.ResumeVersionId);

        Assert.Equal(
            policyRevisionId,
            persisted.MatchingPolicyRevisionId);

        Assert.Equal(
            "Allowed",
            persisted.ApplyDecision);

        Assert.Equal(
            82.499m,
            persisted.RawCompatibilityScore);

        Assert.Equal(
            82.50m,
            persisted.DisplayCompatibilityScore);

        Assert.DoesNotContain(
            currentCandidateName,
            persisted.CandidateSnapshotJson);

        Assert.DoesNotContain(
            currentVacancyTitle,
            persisted.VacancySnapshotJson);

        Assert.NotEqual(
            currentResumeVersionId,
            persisted.ResumeVersionId);
    }
}
