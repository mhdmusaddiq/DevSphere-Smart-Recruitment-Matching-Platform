using DevSphere.Domain.Entities.Administration;
using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Entities;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Career;
using DevSphere.Domain.Entities.Contacts;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Entities.Matching;
using DevSphere.Domain.Entities.Resume;
using DevSphere.Domain.Entities.Vacancies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevSphere.Infrastructure.Configurations;

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.HasOne<CandidateProfile>()
            .WithMany()
            .HasForeignKey(x => x.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Versions)
            .WithOne(x => x.Resume)
            .HasForeignKey(x => x.ResumeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ResumeVersionConfiguration : IEntityTypeConfiguration<ResumeVersion>
{
    public void Configure(EntityTypeBuilder<ResumeVersion> builder)
    {
        builder.Property(x => x.OriginalFileName).HasMaxLength(260);
        builder.Property(x => x.StorageKey).HasMaxLength(500);
        builder.Property(x => x.ContentType).HasMaxLength(200);
        builder.HasIndex(x => new { x.ResumeId, x.VersionNumber }).IsUnique();
    }
}

public class WorkExperienceConfiguration : CandidateRecordConfiguration<WorkExperience>
{
    public override void Configure(EntityTypeBuilder<WorkExperience> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.JobTitle).HasMaxLength(200);
        builder.Property(x => x.CompanyName).HasMaxLength(200);
    }
}

public class EducationRecordConfiguration : CandidateRecordConfiguration<EducationRecord>
{
    public override void Configure(EntityTypeBuilder<EducationRecord> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Institution).HasMaxLength(250);
        builder.Property(x => x.Qualification).HasMaxLength(200);
        builder.Property(x => x.FieldOfStudy).HasMaxLength(200);
    }
}

public class CertificationRecordConfiguration : CandidateRecordConfiguration<CertificationRecord>
{
    public override void Configure(EntityTypeBuilder<CertificationRecord> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Name).HasMaxLength(250);
        builder.Property(x => x.Issuer).HasMaxLength(250);
        builder.Property(x => x.CredentialUrl).HasMaxLength(1000);
    }
}

public class ProjectRecordConfiguration : CandidateRecordConfiguration<ProjectRecord>
{
    public override void Configure(EntityTypeBuilder<ProjectRecord> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Name).HasMaxLength(250);
        builder.Property(x => x.ProjectUrl).HasMaxLength(1000);
    }
}

public class LanguageCapabilityConfiguration : CandidateRecordConfiguration<LanguageCapability>
{
    public override void Configure(EntityTypeBuilder<LanguageCapability> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Language).HasMaxLength(100);
        builder.Property(x => x.Proficiency).HasMaxLength(100);
    }
}

public abstract class CandidateRecordConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasOne<CandidateProfile>()
            .WithMany()
            .HasForeignKey("CandidateProfileId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CompanyVerificationConfiguration : IEntityTypeConfiguration<CompanyVerification>
{
    public void Configure(EntityTypeBuilder<CompanyVerification> builder)
    {
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.EvidenceStorageKey).HasMaxLength(500);
        builder.HasOne<EmployerProfile>()
            .WithMany()
            .HasForeignKey(x => x.EmployerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class VacancyRequirementConfiguration : IEntityTypeConfiguration<VacancyRequirement>
{
    public void Configure(EntityTypeBuilder<VacancyRequirement> builder)
    {
        builder.HasOne<Vacancy>()
            .WithMany()
            .HasForeignKey(x => x.VacancyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RequiredSkillConfiguration : IEntityTypeConfiguration<RequiredSkill>
{
    public void Configure(EntityTypeBuilder<RequiredSkill> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(150);
        builder.HasOne<Vacancy>()
            .WithMany()
            .HasForeignKey(x => x.VacancyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MatchResultConfiguration : IEntityTypeConfiguration<MatchResult>
{
    public void Configure(EntityTypeBuilder<MatchResult> builder)
    {
        builder.HasOne<CandidateProfile>()
            .WithMany()
            .HasForeignKey(x => x.CandidateProfileId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Vacancy>()
            .WithMany()
            .HasForeignKey(x => x.VacancyId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasMany(x => x.CriterionResults)
            .WithOne(x => x.MatchResult)
            .HasForeignKey(x => x.MatchResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CriterionResultConfiguration : IEntityTypeConfiguration<CriterionResult>
{
    public void Configure(EntityTypeBuilder<CriterionResult> builder)
    {
        builder.Property(x => x.CriterionName).HasMaxLength(100);
    }
}

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.Property(x => x.CandidateId)
            .HasMaxLength(450);

        builder.HasOne(x => x.Vacancy)
            .WithMany()
            .HasForeignKey(x => x.VacancyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.CandidateId,
            x.VacancyId
        })
        .IsUnique();
    }
}

public class ContactRequestConfiguration : IEntityTypeConfiguration<ContactRequest>
{
    public void Configure(EntityTypeBuilder<ContactRequest> builder)
    {
        builder.Property(x => x.EmployerId)
            .HasMaxLength(450);

        builder.Property(x => x.CandidateId)
            .HasMaxLength(450);

        builder.Property(x => x.Status)
            .HasMaxLength(50);

        builder.HasOne<JobApplication>()
            .WithMany()
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new
        {
            x.JobApplicationId,
            x.EmployerId
        })
        .IsUnique();

        builder.HasIndex(x => x.CandidateId);
        builder.HasIndex(x => x.EmployerId);
    }
}

public class ApplicationSnapshotConfiguration : IEntityTypeConfiguration<ApplicationSnapshot>
{
    public void Configure(EntityTypeBuilder<ApplicationSnapshot> builder)
    {
        builder.HasOne<JobApplication>()
            .WithMany()
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ResumeVersion>()
            .WithMany()
            .HasForeignKey(x => x.ResumeVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ApplicationStatusHistoryConfiguration : IEntityTypeConfiguration<ApplicationStatusHistory>
{
    public void Configure(EntityTypeBuilder<ApplicationStatusHistory> builder)
    {
        builder.HasOne<JobApplication>()
            .WithMany()
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.ChangedByUserId).HasMaxLength(450);
    }
}

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.Action).HasMaxLength(200);
        builder.Property(x => x.EntityName).HasMaxLength(200);
        builder.Property(x => x.EntityId).HasMaxLength(200);
    }
}

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.Property(x => x.Key).HasMaxLength(200);
        builder.HasIndex(x => x.Key).IsUnique();
    }
}
