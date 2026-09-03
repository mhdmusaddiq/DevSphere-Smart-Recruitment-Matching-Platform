using DevSphere.Application.DTOs.Profile;

namespace DevSphere.Application.Interfaces;

public interface IEmployerProfileService
{
    Task<EmployerProfileDto?> GetByUserIdAsync(string userId);

    Task<EmployerProfileDto> CreateAsync(
        string userId,
        EmployerProfileDto profile);
}