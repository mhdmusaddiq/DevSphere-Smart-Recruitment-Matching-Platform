using DevSphere.Application.DTOs.Profile;
using DevSphere.Application.Interfaces;
using DevSphere.Domain.Entities.Employers;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Profile;

public class EmployerProfileService : IEmployerProfileService
{
    private readonly EmployerProfileRepository _repository;


    public EmployerProfileService(
        EmployerProfileRepository repository)
    {
        _repository = repository;
    }


    public async Task<EmployerProfileDto?> GetByUserIdAsync(
        string userId)
    {
        var profile = await _repository
            .GetByUserIdAsync(userId);


        if (profile == null)
        {
            return null;
        }


        return new EmployerProfileDto
        {
            CompanyName = profile.CompanyName,
            ContactEmail = profile.ContactEmail,
            Website = profile.Website,
            Location = profile.Location
        };
    }



    public async Task<EmployerProfileDto> CreateAsync(
        string userId,
        EmployerProfileDto request)
    {
        var profile = new EmployerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyName = request.CompanyName,
            ContactEmail = request.ContactEmail,
            Website = request.Website,
            Location = request.Location,
            CreatedAt = DateTime.UtcNow
        };


        await _repository.AddAsync(profile);


        return request;
    }
}