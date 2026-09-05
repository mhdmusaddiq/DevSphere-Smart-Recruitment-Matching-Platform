using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Repositories;

namespace DevSphere.Infrastructure.Services.Profile;

public class JobApplicationService : IJobApplicationService
{
    private readonly JobApplicationRepository _repository;


    public JobApplicationService(
        JobApplicationRepository repository)
    {
        _repository = repository;
    }


    public async Task<JobApplicationDto> ApplyAsync(
        JobApplicationDto request)
    {
        var exists = await _repository.ExistsAsync(
            request.CandidateId,
            request.VacancyId);


        if (exists)
        {
            throw new Exception(
                "Application already exists.");
        }


        var application = new DevSphere.Domain.Entities.Applications.JobApplication
        {
            Id = Guid.NewGuid(),
            CandidateId = request.CandidateId,
            VacancyId = request.VacancyId,
            Status = DevSphere.Domain.Enums.ApplicationStatus.Applied,
            AppliedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };


        await _repository.AddAsync(application);


        return new JobApplicationDto
        {
            CandidateId = application.CandidateId,
            VacancyId = application.VacancyId,
            Status = application.Status.ToString()
        };
    }


    public async Task<List<JobApplicationDto>> GetByCandidateAsync(
        string candidateId)
    {
        var applications = await _repository
            .GetByCandidateAsync(candidateId);


        return applications
    .Select(x => new JobApplicationDto
    {
        CandidateId = x.CandidateId,
        VacancyId = x.VacancyId,
        Status = x.Status.ToString()
    })
    .ToList();
    }

    public async Task<List<JobApplicationDto>> GetByVacancyAsync(
    string vacancyId)
    {
        var applications = await _repository
            .GetByVacancyAsync(vacancyId);


        return applications
            .Select(x => new JobApplicationDto
            {
                CandidateId = x.CandidateId,
                VacancyId = x.VacancyId,
                Status = x.Status.ToString()
            })
            .ToList();
    }



    public async Task<JobApplicationDto> UpdateStatusAsync(
        Guid applicationId,
        string status)
    {
        var application = await _repository
            .GetByIdAsync(applicationId);


        if (application == null)
        {
            throw new Exception(
                "Application not found.");
        }


        application.Status =
            Enum.Parse<DevSphere.Domain.Enums.ApplicationStatus>(
                status);


        application.UpdatedAt = DateTime.UtcNow;


        await _repository.UpdateAsync(application);


        return new JobApplicationDto
        {
            CandidateId = application.CandidateId,
            VacancyId = application.VacancyId,
            Status = application.Status.ToString()
        };
    }
}