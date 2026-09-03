using DevSphere.Application.DTOs.Profile;

namespace DevSphere.Application.Interfaces;

public interface ICandidateProfileService
{
    Task<CandidateProfileDto?> GetByUserIdAsync(string userId);

    Task<CandidateProfileDto> CreateAsync(
        string userId,
        CandidateProfileDto profile);
}