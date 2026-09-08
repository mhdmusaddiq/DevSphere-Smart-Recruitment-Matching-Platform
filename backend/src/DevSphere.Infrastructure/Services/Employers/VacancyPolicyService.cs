using DevSphere.Application.DTOs.Employers;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Domain.Enums;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Employers;

public class VacancyPolicyService :
    IVacancyPolicyService
{
    private readonly VacancyPolicyRepository _repository;

    public VacancyPolicyService(
        VacancyPolicyRepository repository)
    {
        _repository = repository;
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
            CanonicalTargetKey =
                requirement.CanonicalTargetKey,
            IsRegulatoryGate =
                requirement.IsRegulatoryGate,
            RequiresVerification =
                requirement.RequiresVerification,
            QuestionText =
                requirement.QuestionText,
            DisplayOrder =
                requirement.DisplayOrder
        };
    }
}


