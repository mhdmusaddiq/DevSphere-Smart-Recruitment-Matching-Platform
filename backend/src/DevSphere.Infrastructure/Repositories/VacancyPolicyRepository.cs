using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class VacancyPolicyRepository
{
    private readonly DevSphereDbContext _context;

    public VacancyPolicyRepository(
        DevSphereDbContext context)
    {
        _context = context;
    }

    public async Task<Vacancy?> GetVacancyAsync(
        Guid vacancyId)
    {
        return await _context.Vacancies
            .FirstOrDefaultAsync(
                x => x.Id == vacancyId);
    }

    public async Task<bool> HasApplicationsAsync(
        Guid vacancyId)
    {
        return await _context.JobApplications
            .AsNoTracking()
            .AnyAsync(
                x => x.VacancyId == vacancyId);
    }

    public async Task<MatchingPolicyRevision?>
        GetCurrentRevisionAsync(
            Guid vacancyId)
    {
        return await _context.MatchingPolicyRevisions
            .FirstOrDefaultAsync(x =>
                x.VacancyId == vacancyId &&
                x.IsCurrent);
    }

    public async Task<int> GetHighestRevisionNumberAsync(
        Guid vacancyId)
    {
        var numbers = await _context.MatchingPolicyRevisions
            .AsNoTracking()
            .Where(x => x.VacancyId == vacancyId)
            .Select(x => x.RevisionNumber)
            .ToListAsync();

        return numbers.Count == 0
            ? 0
            : numbers.Max();
    }

    public async Task<List<FamilyPolicy>>
        GetFamilyPoliciesAsync(
            Guid revisionId)
    {
        return await _context.FamilyPolicies
            .Where(x =>
                x.MatchingPolicyRevisionId == revisionId)
            .ToListAsync();
    }

    public async Task<List<AlternativeSet>>
        GetAlternativeSetsAsync(
            Guid revisionId)
    {
        return await _context.AlternativeSets
            .Where(x =>
                x.MatchingPolicyRevisionId == revisionId)
            .ToListAsync();
    }

    public async Task<List<VacancyRequirement>>
        GetRequirementsAsync(
            Guid revisionId)
    {
        return await _context.VacancyRequirements
            .Where(x =>
                x.MatchingPolicyRevisionId == revisionId)
            .ToListAsync();
    }

    public async Task<FamilyPolicy?> GetFamilyPolicyAsync(
        Guid familyPolicyId)
    {
        return await _context.FamilyPolicies
            .FirstOrDefaultAsync(
                x => x.Id == familyPolicyId);
    }

    public async Task<List<VacancyRequirement>>
        GetRequirementsByIdsAsync(
            IEnumerable<Guid> requirementIds)
    {
        var ids = requirementIds
            .Distinct()
            .ToList();

        return await _context.VacancyRequirements
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<bool> FamilyPolicyExistsAsync(
        Guid revisionId,
        DevSphere.Domain.Enums.RequirementFamily family)
    {
        return await _context.FamilyPolicies
            .AsNoTracking()
            .AnyAsync(x =>
                x.MatchingPolicyRevisionId == revisionId &&
                x.RequirementFamily == family);
    }

    public async Task<bool> CanonicalTargetExistsAsync(
        Guid revisionId,
        DevSphere.Domain.Enums.RequirementFamily family,
        string canonicalTargetKey)
    {
        return await _context.VacancyRequirements
            .AsNoTracking()
            .AnyAsync(x =>
                x.MatchingPolicyRevisionId == revisionId &&
                x.RequirementFamily == family &&
                x.CanonicalTargetKey == canonicalTargetKey);
    }

    public async Task AddRevisionAsync(
        MatchingPolicyRevision revision)
    {
        await _context.MatchingPolicyRevisions
            .AddAsync(revision);

        await _context.SaveChangesAsync();
    }

    public async Task AddFamilyPolicyAsync(
        FamilyPolicy familyPolicy)
    {
        await _context.FamilyPolicies
            .AddAsync(familyPolicy);

        await _context.SaveChangesAsync();
    }

    public async Task AddRequirementAsync(
        VacancyRequirement requirement)
    {
        await _context.VacancyRequirements
            .AddAsync(requirement);

        await _context.SaveChangesAsync();
    }

    public async Task AddAlternativeSetAsync(
        AlternativeSet alternativeSet,
        IEnumerable<VacancyRequirement> members)
    {
        await _context.AlternativeSets
            .AddAsync(alternativeSet);

        foreach (var member in members)
        {
            member.AlternativeSetId =
                alternativeSet.Id;

            member.UpdatedAt =
                DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<MatchingPolicyRevision>
        CloneRevisionAsync(
            Vacancy vacancy,
            MatchingPolicyRevision currentRevision)
    {
        var familyPolicies =
            await GetFamilyPoliciesAsync(
                currentRevision.Id);

        var alternativeSets =
            await GetAlternativeSetsAsync(
                currentRevision.Id);

        var requirements =
            await GetRequirementsAsync(
                currentRevision.Id);

        currentRevision.IsCurrent = false;
        currentRevision.UpdatedAt = DateTime.UtcNow;

        var nextRevision =
            new MatchingPolicyRevision
            {
                Id = Guid.NewGuid(),
                VacancyId = vacancy.Id,
                RevisionNumber =
                    await GetHighestRevisionNumberAsync(
                        vacancy.Id) + 1,
                IsCurrent = true,
                IsMateriallyLocked = false,
                CreatedAt = DateTime.UtcNow
            };

        await _context.MatchingPolicyRevisions
            .AddAsync(nextRevision);

        var familyIdMap =
            new Dictionary<Guid, Guid>();

        foreach (var family in familyPolicies)
        {
            var newFamilyId = Guid.NewGuid();

            familyIdMap[family.Id] =
                newFamilyId;

            await _context.FamilyPolicies.AddAsync(
                new FamilyPolicy
                {
                    Id = newFamilyId,
                    MatchingPolicyRevisionId =
                        nextRevision.Id,
                    RequirementFamily =
                        family.RequirementFamily,
                    FamilyImportance =
                        family.FamilyImportance,
                    IsActive = family.IsActive,
                    IsScored = family.IsScored,
                    CreatedAt = DateTime.UtcNow
                });
        }

        var setIdMap =
            new Dictionary<Guid, Guid>();

        foreach (var set in alternativeSets)
        {
            var newSetId = Guid.NewGuid();

            setIdMap[set.Id] =
                newSetId;

            await _context.AlternativeSets.AddAsync(
                new AlternativeSet
                {
                    Id = newSetId,
                    MatchingPolicyRevisionId =
                        nextRevision.Id,
                    FamilyPolicyId =
                        familyIdMap[set.FamilyPolicyId],
                    SetType = set.SetType,
                    MinimumSatisfiedCount =
                        set.MinimumSatisfiedCount,
                    Mode = set.Mode,
                    Importance = set.Importance,
                    IsActive = set.IsActive,
                    IsScored = set.IsScored,
                    DisplayOrder = set.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                });
        }

        foreach (var requirement in requirements)
        {
            await _context.VacancyRequirements.AddAsync(
                new VacancyRequirement
                {
                    Id = Guid.NewGuid(),
                    VacancyId = vacancy.Id,
                    MatchingPolicyRevisionId =
                        nextRevision.Id,
                    FamilyPolicyId =
                        requirement.FamilyPolicyId.HasValue
                            ? familyIdMap[
                                requirement.FamilyPolicyId.Value]
                            : null,
                    AlternativeSetId =
                        requirement.AlternativeSetId.HasValue
                            ? setIdMap[
                                requirement.AlternativeSetId.Value]
                            : null,
                    RequirementFamily =
                        requirement.RequirementFamily,
                    Mode = requirement.Mode,
                    Importance =
                        requirement.Importance,
                    IsActive =
                        requirement.IsActive,
                    IsScored =
                        requirement.IsScored,
                    Description =
                        requirement.Description,
                    IsMandatory =
                        requirement.IsMandatory,
                    SkillConceptId =
                        requirement.SkillConceptId,
                    CanonicalTargetKey =
                        requirement.CanonicalTargetKey,
                    RequiredMonths =
                        requirement.RequiredMonths,
                    RequiredValue =
                        requirement.RequiredValue,
                    AcceptedValuesJson =
                        requirement.AcceptedValuesJson,
                    IsRegulatoryGate =
                        requirement.IsRegulatoryGate,
                    RequiresVerification =
                        requirement.RequiresVerification,
                    QuestionText =
                        requirement.QuestionText,
                    ExpectedAnswer =
                        requirement.ExpectedAnswer,
                    DisplayOrder =
                        requirement.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                });
        }

        await _context.SaveChangesAsync();

        return nextRevision;
    }
}
