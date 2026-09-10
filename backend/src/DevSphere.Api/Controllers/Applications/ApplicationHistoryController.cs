using System.Security.Claims;
using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Applications;

[ApiController]
[Route("api/applications/{jobApplicationId:guid}")]
[Authorize]
public class ApplicationHistoryController : ControllerBase
{
    private readonly IApplicationHistoryService _service;
    private readonly JobApplicationRepository _applicationRepository;

    public ApplicationHistoryController(
        IApplicationHistoryService service,
        JobApplicationRepository applicationRepository)
    {
        _service = service;
        _applicationRepository = applicationRepository;
    }

    [HttpGet("snapshot")]
    [Authorize(Roles = "Candidate,Employer")]
    public async Task<IActionResult> GetSnapshot(
        Guid jobApplicationId,
        CancellationToken cancellationToken)
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var application =
            await _applicationRepository
                .GetWithVacancyAsync(
                    jobApplicationId);

        if (application == null)
        {
            return NotFound();
        }

        var isCandidate =
            User.IsInRole("Candidate") &&
            application.CandidateId == userId;

        var isEmployer =
            User.IsInRole("Employer") &&
            application.Vacancy.EmployerId == userId;

        if (!isCandidate && !isEmployer)
        {
            return Forbid();
        }

        var snapshot =
            await _service.GetSnapshotAsync(
                jobApplicationId,
                cancellationToken);

        if (snapshot == null)
        {
            return NotFound();
        }

        return Ok(snapshot);
    }
    [HttpGet("status-history")]
    [Authorize(Roles = "Candidate,Employer")]
    public async Task<IActionResult> GetStatusHistory(
        Guid jobApplicationId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var application =
            await _applicationRepository.GetWithVacancyAsync(jobApplicationId);

        if (application == null)
        {
            return NotFound();
        }

        var isCandidate =
            User.IsInRole("Candidate") &&
            application.CandidateId == userId;

        var isEmployer =
            User.IsInRole("Employer") &&
            application.Vacancy.EmployerId == userId;

        if (!isCandidate && !isEmployer)
        {
            return Forbid();
        }

        return Ok(
            await _service.GetStatusHistoryAsync(
                jobApplicationId,
                cancellationToken));
    }

}
