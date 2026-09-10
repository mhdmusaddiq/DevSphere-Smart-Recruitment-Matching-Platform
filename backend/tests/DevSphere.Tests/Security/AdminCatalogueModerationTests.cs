using DevSphere.Domain.Entities.Taxonomy;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevSphere.Tests.Security;

public class AdminCatalogueModerationTests
{
    private static DevSphereDbContext CreateContext()
    {
        var options =
            new DbContextOptionsBuilder<DevSphereDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new DevSphereDbContext(options);
    }

    private static string ReadRepoFile(
        params string[] parts)
    {
        var directory =
            new DirectoryInfo(
                AppContext.BaseDirectory);

        while (directory != null)
        {
            var pathParts =
                new List<string>
                {
                    directory.FullName,
                    "backend"
                };

            pathParts.AddRange(parts);

            var candidate =
                Path.Combine(
                    pathParts.ToArray());

            if (File.Exists(candidate))
            {
                return File.ReadAllText(
                    candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException();
    }

    [Fact]
    public void Admin_Catalogue_Should_Remain_Admin_Only()
    {
        var source =
            ReadRepoFile(
                "src",
                "DevSphere.Api",
                "Controllers",
                "Administration",
                "AdminController.cs");

        Assert.Contains(
            "[Authorize(Roles = \"Admin\")]",
            source);

        Assert.Contains(
            "[HttpGet(\"catalogue/skills\")]",
            source);

        Assert.Contains(
            "[HttpGet(\"catalogue/aliases\")]",
            source);

        Assert.Contains(
            "[HttpGet(\"catalogue/occupations\")]",
            source);
    }

    [Fact]
    public void Admin_Catalogue_Should_Expose_Alias_Moderation_But_No_Score_Write()
    {
        var source =
            ReadRepoFile(
                "src",
                "DevSphere.Api",
                "Controllers",
                "Administration",
                "AdminController.cs");

        Assert.Contains(
            "[HttpPut(\"catalogue/aliases/{aliasId:guid}/status\")]",
            source);

        Assert.DoesNotContain(
            "compatibility-score",
            source);

        Assert.DoesNotContain(
            "matching-score",
            source);
    }

    [Fact]
    public async Task Admin_Should_Toggle_Alias_Status()
    {
        await using var context = CreateContext();
        var concept = new SkillConcept
        {
            Id = Guid.NewGuid(),
            Name = "C Sharp",
            NormalizedName = "C SHARP",
            IsActive = true
        };
        var alias = new SkillAlias
        {
            Id = Guid.NewGuid(),
            SkillConceptId = concept.Id,
            SkillConcept = concept,
            Alias = "C#",
            NormalizedAlias = "C#",
            IsActive = true
        };
        context.AddRange(concept, alias);
        await context.SaveChangesAsync();

        var result = await new AdminRepository(context)
            .SetSkillAliasStatusAsync(alias.Id, false);

        Assert.NotNull(result);
        Assert.False(result!.IsActive);
        Assert.False((await context.SkillAliases.SingleAsync()).IsActive);
    }

    [Fact]
    public async Task Active_Alias_List_Should_Respect_Alias_And_Concept_Status()
    {
        await using var context =
            CreateContext();

        var activeConcept =
            new SkillConcept
            {
                Id = Guid.NewGuid(),
                Name = "C Sharp",
                NormalizedName = "C SHARP",
                IsActive = true
            };

        var inactiveConcept =
            new SkillConcept
            {
                Id = Guid.NewGuid(),
                Name = "Legacy Skill",
                NormalizedName = "LEGACY SKILL",
                IsActive = false
            };

        context.SkillConcepts.AddRange(
            activeConcept,
            inactiveConcept);

        context.SkillAliases.AddRange(
            new SkillAlias
            {
                Id = Guid.NewGuid(),
                SkillConceptId =
                    activeConcept.Id,
                Alias = "C#",
                NormalizedAlias = "C#",
                IsActive = true,
                SkillConcept = activeConcept
            },
            new SkillAlias
            {
                Id = Guid.NewGuid(),
                SkillConceptId =
                    activeConcept.Id,
                Alias = "Inactive Alias",
                NormalizedAlias =
                    "INACTIVE ALIAS",
                IsActive = false,
                SkillConcept = activeConcept
            },
            new SkillAlias
            {
                Id = Guid.NewGuid(),
                SkillConceptId =
                    inactiveConcept.Id,
                Alias = "Legacy Alias",
                NormalizedAlias =
                    "LEGACY ALIAS",
                IsActive = true,
                SkillConcept = inactiveConcept
            });

        await context.SaveChangesAsync();

        var repository =
            new AdminRepository(context);

        var aliases =
            await repository
                .GetSkillAliasesAsync(false);

        Assert.Single(aliases);
        Assert.Equal(
            "C#",
            aliases[0].Alias);
    }

    [Fact]
    public async Task Admin_Should_Toggle_Skill_Concept_Status()
    {
        await using var context =
            CreateContext();

        var concept =
            new SkillConcept
            {
                Id = Guid.NewGuid(),
                Name = "Angular",
                NormalizedName = "ANGULAR",
                IsActive = true
            };

        context.SkillConcepts.Add(concept);

        await context.SaveChangesAsync();

        var repository =
            new AdminRepository(context);

        var result =
            await repository
                .SetSkillConceptStatusAsync(
                    concept.Id,
                    false);

        Assert.NotNull(result);
        Assert.False(result!.IsActive);

        Assert.False(
            (await context.SkillConcepts
                .SingleAsync(
                    x => x.Id == concept.Id))
                .IsActive);
    }

    [Fact]
    public async Task Admin_Should_Toggle_Occupation_Status()
    {
        await using var context =
            CreateContext();

        var occupation =
            new OccupationConcept
            {
                Id = Guid.NewGuid(),
                Name = "Software Engineer",
                NormalizedName =
                    "SOFTWARE ENGINEER",
                IsActive = true
            };

        context.OccupationConcepts.Add(
            occupation);

        await context.SaveChangesAsync();

        var repository =
            new AdminRepository(context);

        var result =
            await repository
                .SetOccupationConceptStatusAsync(
                    occupation.Id,
                    false);

        Assert.NotNull(result);
        Assert.False(result!.IsActive);

        Assert.False(
            (await context.OccupationConcepts
                .SingleAsync(
                    x => x.Id ==
                        occupation.Id))
                .IsActive);
    }
}
