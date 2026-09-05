using DevSphere.Application.DTOs.Application;

namespace DevSphere.Application.Interfaces;

public interface IMatchEngine
{
    Task<MatchResultDto> CalculateAsync(
        string candidateId,
        string vacancyId);
}