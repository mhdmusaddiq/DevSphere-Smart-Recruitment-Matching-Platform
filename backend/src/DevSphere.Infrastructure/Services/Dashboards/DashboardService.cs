using DevSphere.Application.DTOs.Dashboards;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Dashboards;

public class DashboardService : IDashboardService
{
    private readonly DashboardRepository _repository;

    public DashboardService(DashboardRepository repository)
    {
        _repository = repository;
    }

    public Task<CandidateDashboard> GetCandidateAsync(string candidateId, CancellationToken cancellationToken = default)
    {
        return _repository.GetCandidateAsync(candidateId, cancellationToken);
    }

    public Task<EmployerDashboard> GetEmployerAsync(string employerId, CancellationToken cancellationToken = default)
    {
        return _repository.GetEmployerAsync(employerId, cancellationToken);
    }

    public Task<AdminDashboard> GetAdminAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetAdminAsync(cancellationToken);
    }
}
