using DevSphere.Domain.Entities.Administration;
using DevSphere.Domain.Entities.Applications;
using DevSphere.Domain.Entities;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Career;
using DevSphere.Domain.Entities.Contacts;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Domain.Entities.EmployerWorkflow;
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

        builder.HasOne<CompanyProfile>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);
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

        builder.HasOne<MatchingPolicyRevision>()
            .WithMany()
            .HasForeignKey(x => x.MatchingPolicyRevisionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<FamilyPolicy>()
            .WithMany()
            .HasForeignKey(x => x.FamilyPolicyId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<AlternativeSet>()
            .WithMany()
            .HasForeignKey(x => x.AlternativeSetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(x => x.CanonicalTargetKey).HasMaxLength(250);
        builder.Property(x => x.RequiredValue).HasMaxLength(500);
        builder.Property(x => x.QuestionText).HasMaxLength(1000);
        builder.Property(x => x.ExpectedAnswer).HasMaxLength(500);
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


        // One immutable application-time snapshot per application.
        builder.HasIndex(x => x.JobApplicationId)
            .IsUnique();

        builder.Property(x => x.CompatibilityScore)
            .HasPrecision(18, 2);

        builder.Property(x => x.RawCompatibilityScore)
            .HasPrecision(18, 2);

        builder.Property(x => x.DisplayCompatibilityScore)
            .HasPrecision(18, 2);

        builder.Property(x => x.CompatibilityStatus)
            .HasMaxLength(50);

        builder.Property(x => x.EligibilityStatus)
            .HasMaxLength(50);

        builder.Property(x => x.ApplyDecision)
            .HasMaxLength(100);
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

public class LicenceRegistrationConfiguration
    : CandidateRecordConfiguration<LicenceRegistration>
{
    public override void Configure(
        EntityTypeBuilder<LicenceRegistration> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Type)
            .HasMaxLength(150);

        builder.Property(x => x.Class)
            .HasMaxLength(100);

        builder.Property(x => x.Issuer)
            .HasMaxLength(250);

        builder.Property(x => x.Identifier)
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .HasMaxLength(50);

        builder.Property(x => x.VerificationStatus)
            .HasMaxLength(50);

        builder.HasIndex(x => new
        {
            x.CandidateProfileId,
            x.Type,
            x.Identifier
        })
        .IsUnique();
    }
}

public class CompanyProfileConfiguration : IEntityTypeConfiguration<CompanyProfile>
{
    public void Configure(EntityTypeBuilder<CompanyProfile> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Website)
            .HasMaxLength(1000);

        builder.Property(x => x.Location)
            .HasMaxLength(250);
    }
}

public class CompanyMembershipConfiguration : IEntityTypeConfiguration<CompanyMembership>
{
    public void Configure(EntityTypeBuilder<CompanyMembership> builder)
    {
        builder.Property(x => x.EmployerUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasOne<CompanyProfile>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.CompanyId,
            x.EmployerUserId
        }).IsUnique();
    }
}

public class MatchingPolicyRevisionConfiguration : IEntityTypeConfiguration<MatchingPolicyRevision>
{
    public void Configure(EntityTypeBuilder<MatchingPolicyRevision> builder)
    {
        builder.HasOne<Vacancy>()
            .WithMany()
            .HasForeignKey(x => x.VacancyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.VacancyId,
            x.RevisionNumber
        }).IsUnique();
    }
}

public class FamilyPolicyConfiguration : IEntityTypeConfiguration<FamilyPolicy>
{
    public void Configure(EntityTypeBuilder<FamilyPolicy> builder)
    {
        builder.HasOne<MatchingPolicyRevision>()
            .WithMany()
            .HasForeignKey(x => x.MatchingPolicyRevisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.MatchingPolicyRevisionId,
            x.RequirementFamily
        }).IsUnique();
    }
}

public class AlternativeSetConfiguration : IEntityTypeConfiguration<AlternativeSet>
{
    public void Configure(EntityTypeBuilder<AlternativeSet> builder)
    {
        builder.HasOne<MatchingPolicyRevision>()
            .WithMany()
            .HasForeignKey(x => x.MatchingPolicyRevisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<FamilyPolicy>()
            .WithMany()
            .HasForeignKey(x => x.FamilyPolicyId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class InterviewConfiguration :
    IEntityTypeConfiguration<Interview>
{
    public void Configure(
        EntityTypeBuilder<Interview> builder)
    {
        builder.Property(x => x.EmployerUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasOne<JobApplication>()
            .WithMany()
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InterviewSlotConfiguration :
    IEntityTypeConfiguration<InterviewSlot>
{
    public void Configure(
        EntityTypeBuilder<InterviewSlot> builder)
    {
        builder.Property(x => x.LocationOrMeetingUrl)
            .HasMaxLength(1000);

        builder.HasOne<Interview>()
            .WithMany()
            .HasForeignKey(x => x.InterviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ScorecardConfiguration :
    IEntityTypeConfiguration<Scorecard>
{
    public void Configure(
        EntityTypeBuilder<Scorecard> builder)
    {
        builder.Property(x => x.AssessorEmployerUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.HasOne<JobApplication>()
            .WithMany()
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Interview>()
            .WithMany()
            .HasForeignKey(x => x.InterviewId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class OfferConfiguration :
    IEntityTypeConfiguration<Offer>
{
    public void Configure(
        EntityTypeBuilder<Offer> builder)
    {
        builder.Property(x => x.EmployerUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.OfferedSalary)
            .HasPrecision(18, 2);

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.HasOne<JobApplication>()
            .WithMany()
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TalentPoolEntryConfiguration :
    IEntityTypeConfiguration<TalentPoolEntry>
{
    public void Configure(
        EntityTypeBuilder<TalentPoolEntry> builder)
    {
        builder.Property(x => x.CandidateUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.EmployerUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(x => new
        {
            x.JobApplicationId,
            x.EmployerUserId
        })
        .IsUnique();

        builder.HasOne<JobApplication>()
            .WithMany()
            .HasForeignKey(x => x.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
