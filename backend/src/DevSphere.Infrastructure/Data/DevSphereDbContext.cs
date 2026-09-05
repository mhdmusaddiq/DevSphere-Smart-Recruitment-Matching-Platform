using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Contacts;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Entities.Notifications;
using DevSphere.Domain.Entities.Skills;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Data;

public class DevSphereDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public DevSphereDbContext(
        DbContextOptions<DevSphereDbContext> options)
        : base(options)
    {
    }


    public DbSet<Skill> Skills { get; set; }

    public DbSet<CandidateProfile> CandidateProfiles { get; set; }

    public DbSet<EmployerProfile> EmployerProfiles { get; set; }

    public DbSet<Vacancy> Vacancies { get; set; }

    public DbSet<JobApplication> JobApplications { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<ContactRequest> ContactRequests { get; set; }
}