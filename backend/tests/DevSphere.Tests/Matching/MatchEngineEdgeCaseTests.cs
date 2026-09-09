using DevSphere.Infrastructure.Matching;
using DevSphere.Infrastructure.Repositories;
using Xunit;

namespace DevSphere.Tests.Matching;

public class MatchEngineEdgeCaseTests
{
    [Fact]
    public async Task CalculateAsync_Should_Throw_When_Candidate_Not_Found()
    {
        var repository =
            new FakeMissingRepository();

        var engine =
            new MatchEngineService(repository);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            engine.CalculateAsync(
                "invalid",
                Guid.NewGuid().ToString()));
    }
}

public class FakeMissingRepository : MatchingRepository
{
    public FakeMissingRepository()
        : base(null!)
    {
    }

    public override Task<CandidateMatchingEvidence?>
        GetCandidateEvidenceAsync(
            string candidateId,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult<CandidateMatchingEvidence?>(
            null);
    }
}
