using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Career;
using DevSphere.Domain.Entities.Vacancies;
using DevSphere.Infrastructure.Data;
using DevSphere.Infrastructure.Matching;
using Microsoft.EntityFrameworkCore;

namespace DevSphere.Infrastructure.Repositories;

public class MatchingRepository
{
    private readonly DevSphereDbContext _context;

    public MatchingRepository(DevSphereDbContext context)
    {
        _context = context;
    }

    public virtual async Task<CandidateProfile?> GetCandidateAsync(
        string userId)
    {
        return await _context.CandidateProfiles
            .Include(x => x.Skills)
                .ThenInclude(x => x.SkillConcept!)
                    .ThenInclude(x => x.Aliases)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public virtual async Task<Vacancy?> GetVacancyAsync(
        string vacancyId)
    {
        if (!Guid.TryParse(vacancyId, out var id))
        {
            return null;
        }

        return await _context.Vacancies
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public virtual async Task<List<RequiredSkill>> GetRequiredSkillsAsync(
        Guid vacancyId)
    {
        return await _context.RequiredSkills
            .Where(x => x.VacancyId == vacancyId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public virtual async Task<CandidateMatchingEvidence?>
        GetCandidateEvidenceAsync(
            string userId,
            CancellationToken cancellationToken = default)
    {
        var candidate = await _context.CandidateProfiles
            .Include(x => x.Skills)
                .ThenInclude(x => x.SkillConcept!)
                    .ThenInclude(x => x.Aliases)
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (candidate == null)
        {
            return null;
        }

        var profileId = candidate.Id;

        var workExperiences = await _context.WorkExperiences
            .Where(x => x.CandidateProfileId == profileId)
            .OrderBy(x => x.StartDate)
            .ThenBy(x => x.EndDate)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var educationRecords = await _context.EducationRecords
            .Where(x => x.CandidateProfileId == profileId)
            .OrderBy(x => x.StartDate)
            .ThenBy(x => x.EndDate)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var certifications = await _context.CertificationRecords
            .Where(x => x.CandidateProfileId == profileId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Issuer)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var licences = await _context.LicenceRegistrations
            .Where(x => x.CandidateProfileId == profileId)
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Class)
            .ThenBy(x => x.Identifier)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var languages = await _context.LanguageCapabilities
            .Where(x => x.CandidateProfileId == profileId)
            .OrderBy(x => x.Language)
            .ThenBy(x => x.Proficiency)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var projects = await _context.ProjectRecords
            .Where(x => x.CandidateProfileId == profileId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.ProjectUrl)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return new CandidateMatchingEvidence
        {
            Candidate = candidate,
            WorkExperiences = workExperiences,
            EducationRecords = educationRecords,
            Certifications = certifications,
            Licences = licences,
            Languages = languages,
            Projects = projects
        };
    }

    public virtual async Task<VacancyMatchingPolicy?>
        GetVacancyPolicyAsync(
            string vacancyId,
            CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(vacancyId, out var id))
        {
            return null;
        }

        var vacancy = await _context.Vacancies
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (vacancy == null)
        {
            return null;
        }

        var revision = await _context.MatchingPolicyRevisions
            .Where(x =>
                x.VacancyId == id &&
                x.IsCurrent)
            .OrderByDescending(x => x.RevisionNumber)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (revision == null)
        {
            return new VacancyMatchingPolicy
            {
                Vacancy = vacancy
            };
        }

        var families = await _context.FamilyPolicies
            .Where(x =>
                x.MatchingPolicyRevisionId == revision.Id)
            .OrderBy(x => x.RequirementFamily)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var requirements = await _context.VacancyRequirements
            .Where(x =>
                x.MatchingPolicyRevisionId == revision.Id)
            .OrderBy(x => x.RequirementFamily)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var alternativeSets = await _context.AlternativeSets
            .Where(x =>
                x.MatchingPolicyRevisionId == revision.Id)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return new VacancyMatchingPolicy
        {
            Vacancy = vacancy,
            Revision = revision,
            Families = families,
            Requirements = requirements,
            AlternativeSets = alternativeSets
        };
    }
}
