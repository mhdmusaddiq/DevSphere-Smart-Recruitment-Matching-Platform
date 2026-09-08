using DevSphere.Domain.Entities.Skills;
using DevSphere.Domain.Entities.Taxonomy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevSphere.Infrastructure.Data.Configurations;

public class SkillConceptConfiguration
    : IEntityTypeConfiguration<SkillConcept>
{
    public void Configure(
        EntityTypeBuilder<SkillConcept> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.NormalizedName)
            .IsUnique();
    }
}

public class SkillAliasConfiguration
    : IEntityTypeConfiguration<SkillAlias>
{
    public void Configure(
        EntityTypeBuilder<SkillAlias> builder)
    {
        builder.Property(x => x.Alias)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.NormalizedAlias)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.NormalizedAlias)
            .IsUnique();

        builder.HasOne(x => x.SkillConcept)
            .WithMany(x => x.Aliases)
            .HasForeignKey(x => x.SkillConceptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OccupationConceptConfiguration
    : IEntityTypeConfiguration<OccupationConcept>
{
    public void Configure(
        EntityTypeBuilder<OccupationConcept> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.NormalizedName)
            .IsUnique();
    }
}

public class SkillCanonicalConceptConfiguration
    : IEntityTypeConfiguration<Skill>
{
    public void Configure(
        EntityTypeBuilder<Skill> builder)
    {
        builder.HasOne(x => x.SkillConcept)
            .WithMany()
            .HasForeignKey(x => x.SkillConceptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
