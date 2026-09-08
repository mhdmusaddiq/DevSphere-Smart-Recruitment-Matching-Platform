using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Entities.Administration;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Career;
using DevSphere.Domain.Entities.Contacts;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Entities.EmployerWorkflow;
using DevSphere.Domain.Entities.Matching;
using DevSphere.Domain.Entities.Notifications;
using DevSphere.Domain.Entities.Resume;
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

    public DbSet<Resume> Resumes { get; set; }

    public DbSet<ResumeVersion> ResumeVersions { get; set; }

    public DbSet<WorkExperience> WorkExperiences { get; set; }

    public DbSet<EducationRecord> EducationRecords { get; set; }

    public DbSet<CertificationRecord> CertificationRecords { get; set; }

    public DbSet<ProjectRecord> ProjectRecords { get; set; }

    public DbSet<LanguageCapability> LanguageCapabilities { get; set; }

    public DbSet<CompanyProfile> CompanyProfiles { get; set; }
    public DbSet<Interview> Interviews { get; set; }
    public DbSet<InterviewSlot> InterviewSlots { get; set; }
    public DbSet<Scorecard> Scorecards { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<TalentPoolEntry> TalentPoolEntries { get; set; }

    public DbSet<CompanyMembership> CompanyMemberships { get; set; }

    public DbSet<CompanyVerification> CompanyVerifications { get; set; }

    public DbSet<MatchingPolicyRevision> MatchingPolicyRevisions { get; set; }

    public DbSet<FamilyPolicy> FamilyPolicies { get; set; }

    public DbSet<AlternativeSet> AlternativeSets { get; set; }

    public DbSet<VacancyRequirement> VacancyRequirements { get; set; }

    public DbSet<RequiredSkill> RequiredSkills { get; set; }

    public DbSet<MatchResult> MatchResults { get; set; }

    public DbSet<CriterionResult> CriterionResults { get; set; }

    public DbSet<ApplicationSnapshot> ApplicationSnapshots { get; set; }

    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories { get; set; }

    public DbSet<AuditEvent> AuditEvents { get; set; }

    public DbSet<SystemSetting> SystemSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(DevSphereDbContext).Assembly);
    }
}
