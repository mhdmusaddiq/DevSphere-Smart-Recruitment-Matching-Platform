using DevSphere.Application.DTOs.Dashboards;

namespace DevSphere.Application.Interfaces;

public interface IDashboardService
{
    Task<CandidateDashboard> GetCandidateAsync(string candidateId, CancellationToken cancellationToken = default);

    Task<EmployerDashboard> GetEmployerAsync(string employerId, CancellationToken cancellationToken = default);

    Task<AdminDashboard> GetAdminAsync(CancellationToken cancellationToken = default);
}
