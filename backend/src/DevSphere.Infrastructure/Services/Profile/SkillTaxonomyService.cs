using DevSphere.Domain.Entities.Taxonomy;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Services.Profile;

public class SkillTaxonomyService
{
    private readonly DevSphereDbContext _context;

    public SkillTaxonomyService(DevSphereDbContext context)
    {
        _context = context;
    }

    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(
                " ",
                value.Trim()
                    .Split(
                        (char[]?)null,
                        StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
    }

    public async Task<SkillConcept> ResolveOrCreateAsync(
        string suppliedName,
        CancellationToken cancellationToken = default)
    {
        var displayName = string.Join(
            " ",
            suppliedName.Trim()
                .Split(
                    (char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries));

        var normalized = Normalize(displayName);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Skill name is required.");
        }

        var alias = await _context.SkillAliases
            .Include(x => x.SkillConcept)
            .FirstOrDefaultAsync(
                x => x.IsActive &&
                     x.NormalizedAlias == normalized,
                cancellationToken);

        if (alias != null)
        {
            if (!alias.SkillConcept.IsActive)
            {
                throw new InvalidOperationException(
                    "The resolved skill concept is inactive.");
            }

            return alias.SkillConcept;
        }

        var concept = await _context.SkillConcepts
            .FirstOrDefaultAsync(
                x => x.IsActive &&
                     x.NormalizedName == normalized,
                cancellationToken);

        if (concept != null)
        {
            return concept;
        }

        concept = new SkillConcept
        {
            Id = Guid.NewGuid(),
            Name = displayName,
            NormalizedName = normalized,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var canonicalAlias = new SkillAlias
        {
            Id = Guid.NewGuid(),
            SkillConceptId = concept.Id,
            Alias = displayName,
            NormalizedAlias = normalized,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            SkillConcept = concept
        };

        concept.Aliases.Add(canonicalAlias);

        _context.SkillConcepts.Add(concept);

        await _context.SaveChangesAsync(cancellationToken);

        return concept;
    }
}
