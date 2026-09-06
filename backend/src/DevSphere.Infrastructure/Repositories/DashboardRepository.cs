using DevSphere.Application.DTOs.Dashboards;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class DashboardRepository
{
    private readonly DevSphereDbContext _context;

    public DashboardRepository(DevSphereDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateDashboard> GetCandidateAsync(string candidateId, CancellationToken cancellationToken = default)
    {
        return new CandidateDashboard
        {
            CandidateId = candidateId,
            ApplicationCount = await _context.JobApplications.CountAsync(x => x.CandidateId == candidateId, cancellationToken),
            UnreadNotificationCount = await _context.Notifications.CountAsync(x => x.UserId == candidateId && !x.IsRead, cancellationToken),
            PendingContactRequestCount = await _context.ContactRequests.CountAsync(x => x.CandidateId == candidateId && x.Status == "Pending", cancellationToken)
        };
    }

    public async Task<EmployerDashboard> GetEmployerAsync(string employerId, CancellationToken cancellationToken = default)
    {
        return new EmployerDashboard
        {
            EmployerId = employerId,
            OpenVacancyCount = await _context.Vacancies.CountAsync(x => x.EmployerId == employerId && x.IsOpen, cancellationToken),
            ApplicationCount = await _context.JobApplications.CountAsync(x => x.Vacancy.EmployerId == employerId, cancellationToken),
            PendingContactRequestCount = await _context.ContactRequests.CountAsync(x => x.EmployerId == employerId && x.Status == "Pending", cancellationToken)
        };
    }

    public async Task<AdminDashboard> GetAdminAsync(CancellationToken cancellationToken = default)
    {
        return new AdminDashboard
        {
            CandidateCount = await _context.CandidateProfiles.CountAsync(cancellationToken),
            EmployerCount = await _context.EmployerProfiles.CountAsync(cancellationToken),
            OpenVacancyCount = await _context.Vacancies.CountAsync(x => x.IsOpen, cancellationToken),
            ApplicationCount = await _context.JobApplications.CountAsync(cancellationToken),
            PendingVerificationCount = await _context.CompanyVerifications.CountAsync(x => x.Status == "Pending", cancellationToken)
        };
    }
}
