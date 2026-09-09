using DevSphere.Application.DTOs.Profile;

namespace DevSphere.Application.Interfaces;

public interface ICandidateProfileService
{
    Task<CandidateProfileDto?> GetByUserIdAsync(string userId);

    Task<CandidateProfileDto> CreateAsync(
        string userId,
        CandidateProfileDto profile);

    Task<CandidateProfileDto?> UpdateAsync(
        string userId,
        CandidateProfileDto profile);

    Task<IReadOnlyCollection<CandidateSkillDto>> GetSkillsAsync(
        string userId);

    Task<CandidateSkillDto?> AddSkillAsync(
        string userId,
        AddCandidateSkillDto request);

    Task<ApplicationReadinessDto> GetApplicationReadinessAsync(
        string userId);
    Task<ProfileReadinessDto> GetProfileReadinessAsync(
        string userId);
    Task<bool> DeleteSkillAsync(
        string userId,
        Guid skillId);
}
