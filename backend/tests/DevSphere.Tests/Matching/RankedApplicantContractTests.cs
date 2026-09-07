using DevSphere.Application.DTOs.Application;
using Xunit;

namespace DevSphere.Tests.Matching;

public class RankedApplicantContractTests
{
    [Fact]
    public void Applicants_Should_Rank_ByScoreDescending()
    {
        var applicants = new List<RankedApplicantDto>
        {
            new()
            {
                CandidateId = "candidate-low",
                MatchScore = 55
            },
            new()
            {
                CandidateId = "candidate-high",
                MatchScore = 92
            },
            new()
            {
                CandidateId = "candidate-mid",
                MatchScore = 75
            }
        };

        var ranked = applicants
            .OrderByDescending(x => x.MatchScore)
            .ThenBy(
                x => x.CandidateId,
                StringComparer.Ordinal)
            .ToList();

        Assert.Equal(
            [
                "candidate-high",
                "candidate-mid",
                "candidate-low"
            ],
            ranked.Select(x => x.CandidateId));
    }

    [Fact]
    public void Applicants_WithSameScore_Should_UseDeterministicCandidateTieBreak()
    {
        var applicants = new List<RankedApplicantDto>
        {
            new()
            {
                CandidateId = "candidate-c",
                MatchScore = 80
            },
            new()
            {
                CandidateId = "candidate-a",
                MatchScore = 80
            },
            new()
            {
                CandidateId = "candidate-b",
                MatchScore = 80
            }
        };

        var ranked = applicants
            .OrderByDescending(x => x.MatchScore)
            .ThenBy(
                x => x.CandidateId,
                StringComparer.Ordinal)
            .ToList();

        Assert.Equal(
            [
                "candidate-a",
                "candidate-b",
                "candidate-c"
            ],
            ranked.Select(x => x.CandidateId));
    }
}
