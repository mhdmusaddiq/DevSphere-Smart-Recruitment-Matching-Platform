namespace DevSphere.Application.DTOs.Application;

public class ApplicationSnapshotDto
{
    public Guid Id { get; set; }

    public Guid JobApplicationId { get; set; }

    public Guid? ResumeVersionId { get; set; }

    public string CandidateSnapshotJson { get; set; } = string.Empty;

    public string VacancySnapshotJson { get; set; } = string.Empty;

    public DateTime CapturedAtUtc { get; set; }
}

public class ApplicationStatusHistoryDto
{
    public Guid Id { get; set; }

    public Guid JobApplicationId { get; set; }

    public string? PreviousStatus { get; set; }

    public string NewStatus { get; set; } = string.Empty;

    public string ChangedByUserId { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; }

    public string Notes { get; set; } = string.Empty;
}
