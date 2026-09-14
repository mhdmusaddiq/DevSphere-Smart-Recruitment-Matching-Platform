using DevSphere.Application.DTOs.Employers;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Repositories;
using DevSphere.Infrastructure.Data;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Services.Employers;

public class VacancyPolicyService :
    IVacancyPolicyService
{
    private readonly VacancyPolicyRepository _repository;
    private readonly DevSphereDbContext _context;

    public VacancyPolicyService(
        VacancyPolicyRepository repository,
        DevSphereDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task EnsureMaterialRevisionForEditAsync(
        string employerUserId,
        Guid vacancyId)
    {
        var vacancy =
            await GetOwnedVacancyAsync(
                employerUserId,
                vacancyId);

        await GetWritableRevisionAsync(vacancy);
    }

    public async Task<MatchingPolicyRevisionDto>
        GetCurrentRevisionAsync(
            string employerUserId,
            Guid vacancyId)
    {
        var vacancy =
            await GetOwnedVacancyAsync(
                employerUserId,
                vacancyId);

        var revision =
            await EnsureCurrentRevisionAsync(
                vacancy);

        await SynchronizeMaterialLockAsync(
            vacancy,
            revision);

        return MapRevision(revision);
    }

    public async Task<VacancyPolicyAggregateDto>
        GetCurrentAggregateAsync(
            string employerUserId,
            Guid vacancyId)
    {
        var vacancy = await GetOwnedVacancyAsync(
            employerUserId,
            vacancyId);
        var revision = await EnsureCurrentRevisionAsync(vacancy);

        await SynchronizeMaterialLockAsync(vacancy, revision);

        return MapAggregate(
            revision,
            await _repository.GetFamilyPoliciesAsync(revision.Id),
            await _repository.GetRequirementsAsync(revision.Id),
            await _repository.GetAlternativeSetsAsync(revision.Id));
    }

    public async Task<VacancyPolicyAggregateDto>
        ReplaceCurrentAggregateAsync(
            string employerUserId,
            Guid vacancyId,
            VacancyPolicyAggregateUpdateRequest request)
    {
        ValidateAggregate(request);

        await using var transaction = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable)
            : null;

        var vacancy = await GetOwnedVacancyAsync(
            employerUserId,
            vacancyId);
        var revision = await GetWritableRevisionAsync(vacancy);

        var existingRequirements =
            await _repository.GetRequirementsAsync(revision.Id);
        var existingSets =
            await _repository.GetAlternativeSetsAsync(revision.Id);
        var existingFamilies =
            await _repository.GetFamilyPoliciesAsync(revision.Id);

        _context.VacancyRequirements.RemoveRange(existingRequirements);
        _context.AlternativeSets.RemoveRange(existingSets);
        _context.FamilyPolicies.RemoveRange(existingFamilies);
        await _context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var familyIds = request.Families.ToDictionary(
            x => x.ClientKey,
            _ => Guid.NewGuid(),
            StringComparer.OrdinalIgnoreCase);
        var requirementIds = request.Requirements.ToDictionary(
            x => x.ClientKey,
            _ => Guid.NewGuid(),
            StringComparer.OrdinalIgnoreCase);
        var setIds = request.AlternativeSets.ToDictionary(
            x => x.ClientKey,
            _ => Guid.NewGuid(),
            StringComparer.OrdinalIgnoreCase);

        var requirementSetKeys = request.AlternativeSets
            .SelectMany(set => set.MemberRequirementClientKeys
                .Select(requirementKey => new
                {
                    RequirementKey = requirementKey,
                    SetKey = set.ClientKey
                }))
            .ToDictionary(
                x => x.RequirementKey,
                x => x.SetKey,
                StringComparer.OrdinalIgnoreCase);

        var families = request.Families
            .Select(x => new FamilyPolicy
            {
                Id = familyIds[x.ClientKey],
                MatchingPolicyRevisionId = revision.Id,
                RequirementFamily = x.RequirementFamily,
                FamilyImportance = x.FamilyImportance,
                IsActive = x.IsActive,
                IsScored = x.IsScored,
                CreatedAt = now
            })
            .ToList();

        var sets = request.AlternativeSets
            .Select(x => new AlternativeSet
            {
                Id = setIds[x.ClientKey],
                MatchingPolicyRevisionId = revision.Id,
                FamilyPolicyId = familyIds[x.FamilyClientKey],
                SetType = x.SetType,
                MinimumSatisfiedCount = x.SetType == AlternativeSetType.MinSatisfied
                    ? x.MinimumSatisfiedCount
                    : null,
                Mode = x.Mode,
                Importance = x.Importance,
                IsActive = x.IsActive,
                IsScored = x.Mode == RequirementMode.Informational
                    ? false
                    : x.IsScored,
                DisplayOrder = x.DisplayOrder,
                CreatedAt = now
            })
            .ToList();

        var requirements = request.Requirements
            .Select(x => new VacancyRequirement
            {
                Id = requirementIds[x.ClientKey],
                VacancyId = vacancy.Id,
                MatchingPolicyRevisionId = revision.Id,
                FamilyPolicyId = familyIds[x.FamilyClientKey],
                AlternativeSetId = requirementSetKeys.TryGetValue(
                    x.ClientKey,
                    out var setKey)
                        ? setIds[setKey]
                        : null,
                RequirementFamily = x.RequirementFamily,
                Mode = x.Mode,
                Importance = x.Importance,
                IsActive = x.IsActive,
                IsScored = x.Mode == RequirementMode.Informational
                    ? false
                    : x.IsScored,
                Description = x.Description?.Trim() ?? string.Empty,
                IsMandatory = x.Mode == RequirementMode.Mandatory,
                SkillConceptId = x.SkillConceptId,
                CanonicalTargetKey = string.IsNullOrWhiteSpace(x.CanonicalTargetKey)
                    ? null
                    : x.CanonicalTargetKey.Trim(),
                RequiredMonths = x.RequiredMonths,
                RequiredValue = x.RequiredValue?.Trim(),
                AcceptedValuesJson = x.AcceptedValuesJson?.Trim(),
                IsRegulatoryGate = x.IsRegulatoryGate,
                RequiresVerification = x.RequiresVerification,
                QuestionText = x.QuestionText?.Trim(),
                ExpectedAnswer = x.ExpectedAnswer?.Trim(),
                DisplayOrder = x.DisplayOrder,
                CreatedAt = now
            })
            .ToList();

        await _context.FamilyPolicies.AddRangeAsync(families);
        await _context.AlternativeSets.AddRangeAsync(sets);
        await _context.VacancyRequirements.AddRangeAsync(requirements);
        await _context.SaveChangesAsync();

        if (transaction != null)
        {
            await transaction.CommitAsync();
        }

        return MapAggregate(revision, families, requirements, sets);
    }

    public async Task<FamilyPolicyDto>
        AddFamilyPolicyAsync(
            string employerUserId,
            Guid vacancyId,
            FamilyPolicyRequest request)
    {
        var vacancy =
            await GetOwnedVacancyAsync(
                employerUserId,
                vacancyId);

        var revision =
            await GetWritableRevisionAsync(
                vacancy);

        if (await _repository
            .FamilyPolicyExistsAsync(
                revision.Id,
                request.RequirementFamily))
        {
            throw new InvalidOperationException(
                "A family policy already exists for this requirement family.");
        }

        var familyPolicy =
            new FamilyPolicy
            {
                Id = Guid.NewGuid(),
                MatchingPolicyRevisionId =
                    revision.Id,
                RequirementFamily =
                    request.RequirementFamily,
                FamilyImportance =
                    request.FamilyImportance,
                IsActive =
                    request.IsActive,
                IsScored =
                    request.IsScored,
                CreatedAt =
                    DateTime.UtcNow
            };

        await _repository
            .AddFamilyPolicyAsync(
                familyPolicy);

        return new FamilyPolicyDto
        {
            Id = familyPolicy.Id,
            MatchingPolicyRevisionId =
                familyPolicy.MatchingPolicyRevisionId,
            RequirementFamily =
                familyPolicy.RequirementFamily,
            FamilyImportance =
                familyPolicy.FamilyImportance,
            IsActive =
                familyPolicy.IsActive,
            IsScored =
                familyPolicy.IsScored
        };
    }

    public async Task<VacancyRequirementPolicyDto>
        AddRequirementAsync(
            string employerUserId,
            Guid vacancyId,
            VacancyRequirementRequest request)
    {
        var vacancy =
            await GetOwnedVacancyAsync(
                employerUserId,
                vacancyId);

        var revision =
            await GetWritableRevisionAsync(
                vacancy);

        var familyPolicy =
            await _repository.GetFamilyPolicyAsync(
                request.FamilyPolicyId);

        if (familyPolicy == null ||
            familyPolicy.MatchingPolicyRevisionId !=
            revision.Id)
        {
            throw new ArgumentException(
                "Family policy does not belong to the current policy revision.");
        }

        if (familyPolicy.RequirementFamily !=
            request.RequirementFamily)
        {
            throw new ArgumentException(
                "Requirement family must match the family policy.");
        }

        ValidateRequirement(request);

        var canonicalTargetKey =
            string.IsNullOrWhiteSpace(
                request.CanonicalTargetKey)
                ? null
                : request.CanonicalTargetKey.Trim();

        if (canonicalTargetKey != null &&
            await _repository
                .CanonicalTargetExistsAsync(
                    revision.Id,
                    request.RequirementFamily,
                    canonicalTargetKey))
        {
            throw new ArgumentException(
                "Duplicate canonical requirement targets are not allowed within the same family.");
        }

        var isScored =
            request.Mode ==
            RequirementMode.Informational
                ? false
                : request.IsScored;

        var requirement =
            new VacancyRequirement
            {
                Id = Guid.NewGuid(),
                VacancyId = vacancy.Id,
                MatchingPolicyRevisionId =
                    revision.Id,
                FamilyPolicyId =
                    familyPolicy.Id,
                RequirementFamily =
                    request.RequirementFamily,
                Mode =
                    request.Mode,
                Importance =
                    request.Importance,
                IsActive =
                    request.IsActive,
                IsScored =
                    isScored,
                Description =
                    request.Description?.Trim()
                    ?? string.Empty,
                IsMandatory =
                    request.Mode ==
                    RequirementMode.Mandatory,
                SkillConceptId =
                    request.SkillConceptId,
                CanonicalTargetKey =
                    canonicalTargetKey,
                RequiredMonths =
                    request.RequiredMonths,
                RequiredValue =
                    request.RequiredValue?.Trim(),
                AcceptedValuesJson =
                    request.AcceptedValuesJson?.Trim(),
                IsRegulatoryGate =
                    request.IsRegulatoryGate,
                RequiresVerification =
                    request.RequiresVerification,
                QuestionText =
                    request.QuestionText?.Trim(),
                ExpectedAnswer =
                    request.ExpectedAnswer?.Trim(),
                DisplayOrder =
                    request.DisplayOrder,
                CreatedAt =
                    DateTime.UtcNow
            };

        await _repository
            .AddRequirementAsync(
                requirement);

        return MapRequirement(
            requirement);
    }

    public async Task<AlternativeSetDto>
        AddAlternativeSetAsync(
            string employerUserId,
            Guid vacancyId,
            AlternativeSetRequest request)
    {
        var vacancy =
            await GetOwnedVacancyAsync(
                employerUserId,
                vacancyId);

        var revision =
            await GetWritableRevisionAsync(
                vacancy);

        var familyPolicy =
            await _repository.GetFamilyPolicyAsync(
                request.FamilyPolicyId);

        if (familyPolicy == null ||
            familyPolicy.MatchingPolicyRevisionId !=
            revision.Id)
        {
            throw new ArgumentException(
                "Family policy does not belong to the current policy revision.");
        }

        var memberIds =
            request.MemberRequirementIds
                .Distinct()
                .ToList();

        if (memberIds.Count == 0)
        {
            throw new ArgumentException(
                "Alternative set must contain at least one requirement.");
        }

        if (memberIds.Count !=
            request.MemberRequirementIds.Count)
        {
            throw new ArgumentException(
                "Alternative set cannot contain duplicate members.");
        }

        var members =
            await _repository
                .GetRequirementsByIdsAsync(
                    memberIds);

        if (members.Count != memberIds.Count)
        {
            throw new ArgumentException(
                "One or more alternative-set members do not exist.");
        }

        if (members.Any(x =>
            x.MatchingPolicyRevisionId !=
            revision.Id))
        {
            throw new ArgumentException(
                "Alternative-set members must belong to the current policy revision.");
        }

        if (members.Any(x =>
            x.FamilyPolicyId !=
            familyPolicy.Id ||
            x.RequirementFamily !=
            familyPolicy.RequirementFamily))
        {
            throw new ArgumentException(
                "Alternative-set members must belong to one family policy.");
        }

        if (members.Any(x =>
            x.AlternativeSetId.HasValue))
        {
            throw new ArgumentException(
                "A requirement can belong to only one alternative set.");
        }

        var duplicateTargets =
            members
                .Where(x =>
                    !string.IsNullOrWhiteSpace(
                        x.CanonicalTargetKey))
                .GroupBy(
                    x => x.CanonicalTargetKey!,
                    StringComparer.OrdinalIgnoreCase)
                .Any(x => x.Count() > 1);

        if (duplicateTargets)
        {
            throw new ArgumentException(
                "Alternative-set canonical targets must be unique.");
        }

        ValidateAlternativeSet(
            request,
            members.Count);

        var isScored =
            request.Mode ==
            RequirementMode.Informational
                ? false
                : request.IsScored;

        if (members.Any(x =>
            x.Mode != request.Mode ||
            x.Importance != request.Importance ||
            x.IsScored != isScored))
        {
            throw new ArgumentException(
                "Alternative-set members must use the set-level mode, importance and scored state.");
        }

        var alternativeSet =
            new AlternativeSet
            {
                Id = Guid.NewGuid(),
                MatchingPolicyRevisionId =
                    revision.Id,
                FamilyPolicyId =
                    familyPolicy.Id,
                SetType =
                    request.SetType,
                MinimumSatisfiedCount =
                    request.SetType ==
                    AlternativeSetType.MinSatisfied
                        ? request.MinimumSatisfiedCount
                        : null,
                Mode =
                    request.Mode,
                Importance =
                    request.Importance,
                IsActive =
                    request.IsActive,
                IsScored =
                    isScored,
                DisplayOrder =
                    request.DisplayOrder,
                CreatedAt =
                    DateTime.UtcNow
            };

        await _repository
            .AddAlternativeSetAsync(
                alternativeSet,
                members);

        return new AlternativeSetDto
        {
            Id = alternativeSet.Id,
            MatchingPolicyRevisionId =
                alternativeSet
                    .MatchingPolicyRevisionId,
            FamilyPolicyId =
                alternativeSet.FamilyPolicyId,
            SetType =
                alternativeSet.SetType,
            MinimumSatisfiedCount =
                alternativeSet
                    .MinimumSatisfiedCount,
            Mode =
                alternativeSet.Mode,
            Importance =
                alternativeSet.Importance,
            IsActive =
                alternativeSet.IsActive,
            IsScored =
                alternativeSet.IsScored,
            DisplayOrder =
                alternativeSet.DisplayOrder,
            MemberRequirementIds =
                memberIds
        };
    }

    private static void ValidateAggregate(
        VacancyPolicyAggregateUpdateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var familyKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var familyEnums = new HashSet<RequirementFamily>();

        foreach (var family in request.Families)
        {
            if (string.IsNullOrWhiteSpace(family.ClientKey) ||
                !familyKeys.Add(family.ClientKey))
            {
                throw new ArgumentException(
                    "Family client keys are required and must be unique.");
            }

            if (!Enum.IsDefined(family.RequirementFamily) ||
                !Enum.IsDefined(family.FamilyImportance) ||
                !familyEnums.Add(family.RequirementFamily))
            {
                throw new ArgumentException(
                    "Each requirement family may appear only once.");
            }
        }

        var requirementKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var requirementsByKey =
            new Dictionary<string, VacancyRequirementEditRequest>(
                StringComparer.OrdinalIgnoreCase);
        var canonicalTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var requirement in request.Requirements)
        {
            if (string.IsNullOrWhiteSpace(requirement.ClientKey) ||
                !requirementKeys.Add(requirement.ClientKey))
            {
                throw new ArgumentException(
                    "Requirement client keys are required and must be unique.");
            }

            var family = request.Families.SingleOrDefault(x =>
                x.ClientKey.Equals(
                    requirement.FamilyClientKey,
                    StringComparison.OrdinalIgnoreCase));

            if (family == null ||
                family.RequirementFamily != requirement.RequirementFamily)
            {
                throw new ArgumentException(
                    "Every requirement must reference its matching family.");
            }

            if (!Enum.IsDefined(requirement.Mode) ||
                !Enum.IsDefined(requirement.Importance))
            {
                throw new ArgumentException("Invalid requirement enum value.");
            }

            ValidateRequirement(requirement);
            requirementsByKey.Add(requirement.ClientKey, requirement);

            if (!string.IsNullOrWhiteSpace(requirement.CanonicalTargetKey) &&
                !canonicalTargets.Add(
                    $"{requirement.RequirementFamily}:{requirement.CanonicalTargetKey.Trim()}"))
            {
                throw new ArgumentException(
                    "Duplicate canonical requirement targets are not allowed within the same family.");
            }
        }

        var setKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var assignedMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var set in request.AlternativeSets)
        {
            if (string.IsNullOrWhiteSpace(set.ClientKey) ||
                !setKeys.Add(set.ClientKey))
            {
                throw new ArgumentException(
                    "Alternative-set client keys are required and must be unique.");
            }

            var family = request.Families.SingleOrDefault(x =>
                x.ClientKey.Equals(
                    set.FamilyClientKey,
                    StringComparison.OrdinalIgnoreCase));

            if (family == null)
            {
                throw new ArgumentException(
                    "Every alternative set must reference a family.");
            }

            if (!Enum.IsDefined(set.SetType) ||
                !Enum.IsDefined(set.Mode) ||
                !Enum.IsDefined(set.Importance))
            {
                throw new ArgumentException("Invalid alternative-set enum value.");
            }

            var memberKeys = set.MemberRequirementClientKeys
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (memberKeys.Count == 0)
            {
                throw new ArgumentException(
                    "Alternative set must contain at least one requirement.");
            }

            if (memberKeys.Count != set.MemberRequirementClientKeys.Count)
            {
                throw new ArgumentException(
                    "Alternative sets cannot contain duplicate members.");
            }

            ValidateAlternativeSet(set, memberKeys.Count);
            var expectedScored = set.Mode == RequirementMode.Informational
                ? false
                : set.IsScored;

            foreach (var memberKey in memberKeys)
            {
                if (!requirementsByKey.TryGetValue(memberKey, out var member) ||
                    !member.FamilyClientKey.Equals(
                        set.FamilyClientKey,
                        StringComparison.OrdinalIgnoreCase) ||
                    member.RequirementFamily != family.RequirementFamily)
                {
                    throw new ArgumentException(
                        "Alternative-set members must belong to the referenced family.");
                }

                var memberIsScored = member.Mode == RequirementMode.Informational
                    ? false
                    : member.IsScored;

                if (member.Mode != set.Mode ||
                    member.Importance != set.Importance ||
                    memberIsScored != expectedScored)
                {
                    throw new ArgumentException(
                        "Alternative-set members must use the set-level mode, importance and scored state.");
                }

                if (!assignedMembers.Add(memberKey))
                {
                    throw new ArgumentException(
                        "A requirement can belong to only one alternative set.");
                }
            }
        }
    }

    private static VacancyPolicyAggregateDto MapAggregate(
        MatchingPolicyRevision revision,
        IEnumerable<FamilyPolicy> families,
        IEnumerable<VacancyRequirement> requirements,
        IEnumerable<AlternativeSet> alternativeSets)
    {
        var requirementList = requirements.ToList();

        return new VacancyPolicyAggregateDto
        {
            Revision = MapRevision(revision),
            Families = families
                .OrderBy(x => x.RequirementFamily)
                .Select(MapFamily)
                .ToList(),
            Requirements = requirementList
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .Select(MapRequirement)
                .ToList(),
            AlternativeSets = alternativeSets
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .Select(set => new AlternativeSetDto
                {
                    Id = set.Id,
                    MatchingPolicyRevisionId = set.MatchingPolicyRevisionId,
                    FamilyPolicyId = set.FamilyPolicyId,
                    SetType = set.SetType,
                    MinimumSatisfiedCount = set.MinimumSatisfiedCount,
                    Mode = set.Mode,
                    Importance = set.Importance,
                    IsActive = set.IsActive,
                    IsScored = set.IsScored,
                    DisplayOrder = set.DisplayOrder,
                    MemberRequirementIds = requirementList
                        .Where(x => x.AlternativeSetId == set.Id)
                        .OrderBy(x => x.DisplayOrder)
                        .Select(x => x.Id)
                        .ToList()
                })
                .ToList()
        };
    }

    private static FamilyPolicyDto MapFamily(FamilyPolicy family)
    {
        return new FamilyPolicyDto
        {
            Id = family.Id,
            MatchingPolicyRevisionId = family.MatchingPolicyRevisionId,
            RequirementFamily = family.RequirementFamily,
            FamilyImportance = family.FamilyImportance,
            IsActive = family.IsActive,
            IsScored = family.IsScored
        };
    }

    private async Task<Vacancy>
        GetOwnedVacancyAsync(
            string employerUserId,
            Guid vacancyId)
    {
        if (string.IsNullOrWhiteSpace(
            employerUserId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated employer is required.");
        }

        var vacancy =
            await _repository.GetVacancyAsync(
                vacancyId);

        if (vacancy == null)
        {
            throw new KeyNotFoundException(
                "Vacancy not found.");
        }

        if (vacancy.EmployerId !=
            employerUserId)
        {
            throw new UnauthorizedAccessException(
                "You cannot manage this vacancy policy.");
        }

        return vacancy;
    }

    private async Task<MatchingPolicyRevision>
        EnsureCurrentRevisionAsync(
            Vacancy vacancy)
    {
        var current =
            await _repository
                .GetCurrentRevisionAsync(
                    vacancy.Id);

        if (current != null)
        {
            return current;
        }

        var revision =
            new MatchingPolicyRevision
            {
                Id = Guid.NewGuid(),
                VacancyId = vacancy.Id,
                RevisionNumber = 1,
                IsCurrent = true,
                IsMateriallyLocked = false,
                CreatedAt = DateTime.UtcNow
            };

        await _repository
            .AddRevisionAsync(revision);

        return revision;
    }

    private async Task<MatchingPolicyRevision>
        GetWritableRevisionAsync(
            Vacancy vacancy)
    {
        if (vacancy.LifecycleStatus ==
            VacancyLifecycleStatus.Closed)
        {
            throw new InvalidOperationException(
                "Closed vacancy policy cannot be changed.");
        }

        var current =
            await EnsureCurrentRevisionAsync(
                vacancy);

        var hasApplications =
            await _repository
                .HasApplicationsAsync(
                    vacancy.Id);

        if (hasApplications)
        {
            if (!current.IsMateriallyLocked)
            {
                current.IsMateriallyLocked =
                    true;

                current.MateriallyLockedAtUtc =
                    DateTime.UtcNow;

                current.UpdatedAt =
                    DateTime.UtcNow;

                await _repository.SaveAsync();
            }

            throw new InvalidOperationException(
                "Material vacancy policy is locked after the first application.");
        }

        if (current.IsMateriallyLocked)
        {
            throw new InvalidOperationException(
                "Material vacancy policy is locked.");
        }

        if (vacancy.LifecycleStatus ==
            VacancyLifecycleStatus.Published)
        {
            // Revision 1 represents the policy that existed when the
            // vacancy was first published. The first material edit after
            // publication creates one successor revision. Further edits
            // continue on that current successor until the first
            // application locks it.
            if (current.RevisionNumber == 1)
            {
                return await _repository
                    .CloneRevisionAsync(
                        vacancy,
                        current);
            }

            return current;
        }

        return current;
    }

    private async Task SynchronizeMaterialLockAsync(
        Vacancy vacancy,
        MatchingPolicyRevision revision)
    {
        if (revision.IsMateriallyLocked)
        {
            return;
        }

        var hasApplications =
            await _repository
                .HasApplicationsAsync(
                    vacancy.Id);

        if (!hasApplications)
        {
            return;
        }

        revision.IsMateriallyLocked = true;
        revision.MateriallyLockedAtUtc =
            DateTime.UtcNow;
        revision.UpdatedAt =
            DateTime.UtcNow;

        await _repository.SaveAsync();
    }

    private static void ValidateRequirement(
        VacancyRequirementRequest request)
    {
        if (request.RequiredMonths.HasValue &&
            request.RequiredMonths.Value < 0)
        {
            throw new ArgumentException(
                "Required months cannot be negative.");
        }

        if (request.Mode ==
            RequirementMode.Informational &&
            request.IsRegulatoryGate)
        {
            throw new ArgumentException(
                "Informational requirements cannot be regulatory gates.");
        }

        if (request.RequirementFamily ==
            RequirementFamily.StructuredQuestion)
        {
            if (string.IsNullOrWhiteSpace(
                request.QuestionText))
            {
                throw new ArgumentException(
                    "Structured question text is required.");
            }

            if (request.Mode !=
                    RequirementMode.Informational &&
                request.IsScored &&
                string.IsNullOrWhiteSpace(
                    request.ExpectedAnswer))
            {
                throw new ArgumentException(
                    "A scored structured question requires an expected answer.");
            }
        }
    }

    private static void ValidateAlternativeSet(
        AlternativeSetRequest request,
        int memberCount)
    {
        if (request.SetType ==
            AlternativeSetType.AnyOf)
        {
            if (request.MinimumSatisfiedCount
                .HasValue)
            {
                throw new ArgumentException(
                    "AnyOf alternative sets cannot define MinimumSatisfiedCount.");
            }

            return;
        }

        if (!request.MinimumSatisfiedCount
            .HasValue)
        {
            throw new ArgumentException(
                "MinSatisfied alternative sets require MinimumSatisfiedCount.");
        }

        var minimum =
            request.MinimumSatisfiedCount.Value;

        if (minimum < 1 ||
            minimum > memberCount)
        {
            throw new ArgumentException(
                "MinimumSatisfiedCount must be between 1 and the member count.");
        }
    }

    private static MatchingPolicyRevisionDto
        MapRevision(
            MatchingPolicyRevision revision)
    {
        return new MatchingPolicyRevisionDto
        {
            Id = revision.Id,
            VacancyId = revision.VacancyId,
            RevisionNumber =
                revision.RevisionNumber,
            IsCurrent =
                revision.IsCurrent,
            IsMateriallyLocked =
                revision.IsMateriallyLocked,
            MateriallyLockedAtUtc =
                revision.MateriallyLockedAtUtc
        };
    }

    private static VacancyRequirementPolicyDto
        MapRequirement(
            VacancyRequirement requirement)
    {
        return new VacancyRequirementPolicyDto
        {
            Id = requirement.Id,
            VacancyId =
                requirement.VacancyId,
            MatchingPolicyRevisionId =
                requirement
                    .MatchingPolicyRevisionId!.Value,
            FamilyPolicyId =
                requirement
                    .FamilyPolicyId!.Value,
            AlternativeSetId =
                requirement.AlternativeSetId,
            RequirementFamily =
                requirement.RequirementFamily,
            Mode =
                requirement.Mode,
            Importance =
                requirement.Importance,
            IsActive =
                requirement.IsActive,
            IsScored =
                requirement.IsScored,
            Description =
                requirement.Description,
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
                requirement.DisplayOrder
        };
    }
}
