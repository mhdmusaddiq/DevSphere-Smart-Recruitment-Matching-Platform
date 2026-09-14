namespace DevSphere.Application.DTOs.Dashboards;

public class CandidateDashboard
{
    public string CandidateId { get; set; } = string.Empty;

    public int ApplicationCount { get; set; }

    public int UnreadNotificationCount { get; set; }

    public int PendingContactRequestCount { get; set; }
}
