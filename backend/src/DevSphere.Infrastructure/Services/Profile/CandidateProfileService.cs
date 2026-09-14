using DevSphere.Application.DTOs.Profile;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Skills;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Profile;

public class CandidateProfileService : ICandidateProfileService
{
    private readonly CandidateProfileRepository _repository;
    private readonly SkillTaxonomyService _taxonomy;
    private readonly ResumeRepository _resumeRepository;

    public CandidateProfileService(
        CandidateProfileRepository repository,
        SkillTaxonomyService taxonomy,
        ResumeRepository resumeRepository)
    {
        _repository = repository;
        _taxonomy = taxonomy;
        _resumeRepository = resumeRepository;
    }


    public async Task<CandidateProfileDto?> GetByUserIdAsync(
        string userId)
    {
        var profile = await _repository
            .GetByUserIdAsync(userId);

        if (profile == null)
        {
            return null;
        }

        return new CandidateProfileDto
        {
            FullName = profile.FullName,
            Location = profile.Location,
            ExperienceMonths = profile.ExperienceMonths,
            Education = profile.Education,
            PreferredWorkMode = NormalizeWorkMode(profile.PreferredWorkMode),
            PreferredLocation = profile.PreferredLocation?.Trim() ?? string.Empty,
            WillingToRelocate = profile.WillingToRelocate,
            PreferredEmploymentType = profile.PreferredEmploymentType,
            AvailabilityStatus = profile.AvailabilityStatus,
            AvailableFrom = profile.AvailableFrom,
            NoticePeriodDays = profile.NoticePeriodDays
        };
    }


    public async Task<CandidateProfileDto> CreateAsync(
        string userId,
        CandidateProfileDto profile)
    {
        var existing = await _repository.GetByUserIdAsync(userId);

        if (existing != null)
        {
            throw new InvalidOperationException(
                "A candidate profile already exists for this user.");
        }

        var entity = new CandidateProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = profile.FullName,
            Location = profile.Location,
            ExperienceMonths = profile.ExperienceMonths,
            Education = profile.Education,
            PreferredWorkMode = NormalizeWorkMode(profile.PreferredWorkMode),
            PreferredLocation = profile.PreferredLocation?.Trim() ?? string.Empty,
            WillingToRelocate = profile.WillingToRelocate,
            PreferredEmploymentType = NormalizeEmploymentType(profile.PreferredEmploymentType),
            AvailabilityStatus = NormalizeAvailabilityStatus(profile.AvailabilityStatus),
            AvailableFrom = profile.AvailableFrom,
            NoticePeriodDays = NormalizeNoticePeriod(profile.NoticePeriodDays),
            CreatedAt = DateTime.UtcNow
        };


        var saved = await _repository
            .AddAsync(entity);


        return new CandidateProfileDto
        {
            FullName = saved.FullName,
            Location = saved.Location,
            ExperienceMonths = saved.ExperienceMonths,
            Education = saved.Education,
            PreferredWorkMode = saved.PreferredWorkMode,
            PreferredLocation = saved.PreferredLocation,
            WillingToRelocate = saved.WillingToRelocate,
            PreferredEmploymentType = saved.PreferredEmploymentType,
            AvailabilityStatus = saved.AvailabilityStatus,
            AvailableFrom = saved.AvailableFrom,
            NoticePeriodDays = saved.NoticePeriodDays
        };
    }

    public async Task<CandidateProfileDto?> UpdateAsync(
        string userId,
        CandidateProfileDto profile)
    {
        var existing = await _repository.GetByUserIdAsync(userId);

        if (existing == null)
        {
            return null;
        }

        existing.FullName = profile.FullName;
        existing.Location = profile.Location;
        existing.ExperienceMonths = profile.ExperienceMonths;
        existing.Education = profile.Education;
        existing.PreferredWorkMode =
            NormalizeWorkMode(profile.PreferredWorkMode);
        existing.PreferredLocation =
            profile.PreferredLocation?.Trim() ?? string.Empty;
        existing.WillingToRelocate =
            profile.WillingToRelocate;
        existing.PreferredEmploymentType =
            NormalizeEmploymentType(profile.PreferredEmploymentType);

        existing.AvailabilityStatus =
            NormalizeAvailabilityStatus(profile.AvailabilityStatus);
        existing.AvailableFrom =
            profile.AvailableFrom;
        existing.NoticePeriodDays =
            NormalizeNoticePeriod(profile.NoticePeriodDays);

        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);

        return new CandidateProfileDto
        {
            FullName = updated.FullName,
            Location = updated.Location,
            ExperienceMonths = updated.ExperienceMonths,
            Education = updated.Education,
            PreferredWorkMode = updated.PreferredWorkMode,
            PreferredLocation = updated.PreferredLocation,
            WillingToRelocate = updated.WillingToRelocate,
            PreferredEmploymentType = updated.PreferredEmploymentType,
            AvailabilityStatus = updated.AvailabilityStatus,
            AvailableFrom = updated.AvailableFrom,
            NoticePeriodDays = updated.NoticePeriodDays
        };
    }
    public async Task<IReadOnlyCollection<CandidateSkillDto>> GetSkillsAsync(
        string userId)
    {
        var profile = await _repository
            .GetByUserIdWithSkillsAsync(userId);

        if (profile == null)
        {
            return Array.Empty<CandidateSkillDto>();
        }

        return profile.Skills
            .OrderBy(x => x.Name)
            .Select(x => new CandidateSkillDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToList();
    }

    public async Task<CandidateSkillDto?> AddSkillAsync(
        string userId,
        AddCandidateSkillDto request)
    {
        var normalizedName = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException(
                "Skill name is required.");
        }

        var profile = await _repository
            .GetByUserIdWithSkillsAsync(userId);

        if (profile == null)
        {
            return null;
        }

        var duplicate = profile.Skills.Any(x =>
            string.Equals(
                x.Name.Trim(),
                normalizedName,
                StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            throw new InvalidOperationException(
                "This skill already exists on the candidate profile.");
        }

        var concept = await _taxonomy.ResolveOrCreateAsync(
            normalizedName);

        var canonicalDuplicate = profile.Skills.Any(x =>
            x.SkillConceptId == concept.Id);

        if (canonicalDuplicate)
        {
            throw new InvalidOperationException(
                "This skill already exists on the candidate profile.");
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = concept.Name,
            SkillConceptId = concept.Id,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _repository
            .AddSkillAsync(profile, skill);

        return new CandidateSkillDto
        {
            Id = saved.Id,
            Name = saved.Name
        };
    }

    public async Task<bool> DeleteSkillAsync(
        string userId,
        Guid skillId)
    {
        var profile = await _repository
            .GetByUserIdWithSkillsAsync(userId);

        if (profile == null)
        {
            return false;
        }

        return await _repository
            .RemoveSkillAsync(profile, skillId);
    }

    private static string NormalizeWorkMode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Trim();

        if (normalized.Equals(
            "Remote",
            StringComparison.OrdinalIgnoreCase))
            return "Remote";

        if (normalized.Equals(
            "Hybrid",
            StringComparison.OrdinalIgnoreCase))
            return "Hybrid";

        if (normalized.Equals(
            "Onsite",
            StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals(
                "On-site",
                StringComparison.OrdinalIgnoreCase))
            return "Onsite";

        throw new ArgumentException(
            "Preferred work mode must be Remote, Hybrid or Onsite.");
    }

    private static string NormalizeEmploymentType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value
            .Trim()
            .Replace("-", "")
            .Replace(" ", "");

        if (normalized.Equals(
            "FullTime",
            StringComparison.OrdinalIgnoreCase))
            return "FullTime";

        if (normalized.Equals(
            "PartTime",
            StringComparison.OrdinalIgnoreCase))
            return "PartTime";

        if (normalized.Equals(
            "Contract",
            StringComparison.OrdinalIgnoreCase))
            return "Contract";

        if (normalized.Equals(
            "Internship",
            StringComparison.OrdinalIgnoreCase))
            return "Internship";

        throw new ArgumentException(
            "Preferred employment type must be FullTime, PartTime, Contract or Internship.");
    }

    private static string NormalizeAvailabilityStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value
            .Trim()
            .Replace("-", "")
            .Replace(" ", "");

        if (normalized.Equals(
            "Immediately",
            StringComparison.OrdinalIgnoreCase))
            return "Immediately";

        if (normalized.Equals(
            "NoticePeriod",
            StringComparison.OrdinalIgnoreCase))
            return "NoticePeriod";

        if (normalized.Equals(
            "SpecificDate",
            StringComparison.OrdinalIgnoreCase))
            return "SpecificDate";

        if (normalized.Equals(
            "Unavailable",
            StringComparison.OrdinalIgnoreCase))
            return "Unavailable";

        throw new ArgumentException(
            "Availability status must be Immediately, NoticePeriod, SpecificDate or Unavailable.");
    }

    private static int? NormalizeNoticePeriod(int? days)
    {
        if (!days.HasValue)
            return null;

        if (days.Value < 0 || days.Value > 365)
        {
            throw new ArgumentException(
                "Notice period must be between 0 and 365 days.");
        }

        return days.Value;
    }

    public async Task<ProfileReadinessDto> GetProfileReadinessAsync(
        string userId)
    {
        var profile = await _repository
            .GetByUserIdWithSkillsAsync(userId);

        var missingItems = new List<string>();

        if (profile == null)
        {
            missingItems.Add("Profile");

            return new ProfileReadinessDto
            {
                IsReady = false,
                MissingItems = missingItems
            };
        }

        if (string.IsNullOrWhiteSpace(profile.FullName))
            missingItems.Add("FullName");

        if (string.IsNullOrWhiteSpace(profile.Education))
            missingItems.Add("Education");

        if (profile.ExperienceMonths < 0)
            missingItems.Add("Experience");

        if (!profile.Skills.Any(
            x => x.SkillConceptId.HasValue))
        {
            missingItems.Add("CanonicalSkill");
        }

        if (string.IsNullOrWhiteSpace(
            profile.PreferredWorkMode))
        {
            missingItems.Add("PreferredWorkMode");
        }

        if (string.IsNullOrWhiteSpace(
            profile.PreferredLocation))
        {
            missingItems.Add("PreferredLocation");
        }

        if (string.IsNullOrWhiteSpace(
            profile.PreferredEmploymentType))
        {
            missingItems.Add("PreferredEmploymentType");
        }

        if (string.IsNullOrWhiteSpace(
            profile.AvailabilityStatus))
        {
            missingItems.Add("AvailabilityStatus");
        }

        if (profile.AvailabilityStatus.Equals(
                "SpecificDate",
                StringComparison.OrdinalIgnoreCase) &&
            !profile.AvailableFrom.HasValue)
        {
            missingItems.Add("AvailableFrom");
        }

        if (profile.AvailabilityStatus.Equals(
                "NoticePeriod",
                StringComparison.OrdinalIgnoreCase) &&
            !profile.NoticePeriodDays.HasValue)
        {
            missingItems.Add("NoticePeriodDays");
        }

        return new ProfileReadinessDto
        {
            IsReady = missingItems.Count == 0,
            MissingItems = missingItems
        };
    }

    public async Task<ApplicationReadinessDto> GetApplicationReadinessAsync(
        string userId)
    {
        var profileReadiness = await GetProfileReadinessAsync(userId);

        var result = new ApplicationReadinessDto
        {
            ProfileReady = profileReadiness.IsReady
        };

        foreach (var item in profileReadiness.MissingItems)
        {
            result.MissingItems.Add(item);
        }

        var profile = await _repository.GetByUserIdAsync(userId);

        if (profile == null)
        {
            result.MissingItems.Add("Resume");
            result.ResumeReady = false;
            result.IsReady = false;

            return result;
        }

        var resume = await _resumeRepository
            .GetByCandidateProfileIdAsync(profile.Id);

        if (resume == null)
        {
            result.MissingItems.Add("Resume");
        }
        else if (!resume.CurrentVersionId.HasValue)
        {
            result.MissingItems.Add("CurrentResumeVersion");
        }
        else
        {
            var currentVersion = resume.Versions.FirstOrDefault(
                x => x.Id == resume.CurrentVersionId.Value &&
                     x.IsCurrent);

            if (currentVersion == null)
            {
                result.MissingItems.Add("CurrentResumeVersion");
            }
            else
            {
                result.ResumeReady = true;
                result.CurrentResumeVersionId = currentVersion.Id;
            }
        }

        result.IsReady =
            result.ProfileReady &&
            result.ResumeReady &&
            result.MissingItems.Count == 0;

        return result;
    }
}
