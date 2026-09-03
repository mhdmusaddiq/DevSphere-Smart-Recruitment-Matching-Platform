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
        CandidateProfileDto request)
    {
        var profile = new CandidateProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = request.FullName,
            Location = request.Location,
            ExperienceMonths = request.ExperienceMonths,
            Education = request.Education,
            CreatedAt = DateTime.UtcNow
        };


        await _repository.AddAsync(profile);


        return request;
    }
}