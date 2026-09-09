namespace DevSphere.Domain.Enums;

public enum ApplicationStatus
{
    Submitted = 1,
    Applied = Submitted,
    UnderReview = 2,
    Shortlisted = 3,
    Selected = 4,
    Rejected = 5,
    Screening = 6,
    Withdrawn = 7
}
