using DevSphere.Application.DTOs.Profile;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Candidates;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Profile;

public class CandidateProfileService : ICandidateProfileService
{
    private readonly CandidateProfileRepository _repository;

    public CandidateProfileService(
        CandidateProfileRepository repository)
    {
        _repository = repository;
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
            Education = profile.Education
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
            CreatedAt = DateTime.UtcNow
        };


        var saved = await _repository
            .AddAsync(entity);


        return new CandidateProfileDto
        {
            FullName = saved.FullName,
            Location = saved.Location,
            ExperienceMonths = saved.ExperienceMonths,
            Education = saved.Education
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
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);

        return new CandidateProfileDto
        {
            FullName = updated.FullName,
            Location = updated.Location,
            ExperienceMonths = updated.ExperienceMonths,
            Education = updated.Education
        };
    }}

