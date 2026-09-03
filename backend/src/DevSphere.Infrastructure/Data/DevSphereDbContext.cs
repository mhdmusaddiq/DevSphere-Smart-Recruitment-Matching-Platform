using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using DevSphere.Infrastructure.Identity;

using DevSphere.Domain.Entities.Skills;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Entities.Applications;

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
}