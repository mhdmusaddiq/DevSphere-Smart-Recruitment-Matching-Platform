using DevSphere.Domain.Enums;

namespace DevSphere.Domain.Entities.Employers;

public class CompanyMembership : BaseEntity
{
    public Guid CompanyId { get; set; }

    public string EmployerUserId { get; set; } = string.Empty;

    public CompanyMembershipStatus Status { get; set; }
        = CompanyMembershipStatus.Pending;
}
