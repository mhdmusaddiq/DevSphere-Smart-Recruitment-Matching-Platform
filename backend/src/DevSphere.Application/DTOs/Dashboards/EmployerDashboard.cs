namespace DevSphere.Application.DTOs.Dashboards;

public class EmployerDashboard
{
    public string EmployerId { get; set; } = string.Empty;

    public int OpenVacancyCount { get; set; }

    public int ApplicationCount { get; set; }

    public int PendingContactRequestCount { get; set; }
}
