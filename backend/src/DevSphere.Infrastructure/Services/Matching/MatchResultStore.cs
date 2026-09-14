using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Matching;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Matching;

public class MatchResultStore : IMatchResultStore
{
    private readonly MatchResultRepository _repository;

    public MatchResultStore(MatchResultRepository repository)
    {
        _repository = repository;
    }

    public async Task<PersistedMatchResultDto> SaveAsync(PersistedMatchResultDto request, CancellationToken cancellationToken = default)
    {
        var entity = new MatchResult
        {
            Id = Guid.NewGuid(),
            CandidateProfileId = request.CandidateProfileId,
            VacancyId = request.VacancyId,
            MatchedSkills = request.MatchedSkills,
            MissingSkills = request.MissingSkills,
            FinalScore = request.FinalScore,
            CalculatedAtUtc = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);
        request.Id = entity.Id;
        request.CalculatedAtUtc = entity.CalculatedAtUtc;
        return request;
    }
}
