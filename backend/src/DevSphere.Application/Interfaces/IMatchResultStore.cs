using DevSphere.Application.DTOs.Application;

namespace DevSphere.Application.Interfaces;

public interface IMatchResultStore
{
    Task<PersistedMatchResultDto> SaveAsync(PersistedMatchResultDto result, CancellationToken cancellationToken = default);
}
