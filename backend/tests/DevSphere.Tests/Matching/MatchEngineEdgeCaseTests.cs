using DevSphere.Infrastructure.Matching;
using DevSphere.Infrastructure.Repositories;
using Xunit;

namespace DevSphere.Tests.Matching;

public class MatchEngineEdgeCaseTests
{
    [Fact]
    public async Task CalculateAsync_Should_Throw_When_Candidate_Not_Found()
    {
        var repository = new FakeMissingRepository();

        var engine = new MatchEngineService(repository);

        await Assert.ThrowsAsync<Exception>(() =>
            engine.CalculateAsync(
                "invalid",
                "vacancy"));
    }
}


public class FakeMissingRepository : MatchingRepository
{
    public FakeMissingRepository()
        : base(null!)
    {
    }


    public override Task<DevSphere.Domain.Entities.Candidates.CandidateProfile?> GetCandidateAsync(
        string candidateId)
    {
        return Task.FromResult<DevSphere.Domain.Entities.Candidates.CandidateProfile?>(null);
    }


    public override Task<DevSphere.Domain.Entities.Vacancies.Vacancy?> GetVacancyAsync(
        string vacancyId)
    {
        return Task.FromResult<DevSphere.Domain.Entities.Vacancies.Vacancy?>(null);
    }
}