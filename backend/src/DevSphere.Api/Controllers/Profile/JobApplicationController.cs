using DevSphere.Application.Exceptions;
using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using DevSphere.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevSphere.Api.Controllers.Profile;

[ApiController]
[Route("api/applications")]
public class JobApplicationController : ControllerBase
{
    private readonly IJobApplicationService _service;
    private readonly JobApplicationRepository _repository;


    public JobApplicationController(
        IJobApplicationService service,
        JobApplicationRepository repository)
    {
        _service = service;
        _repository = repository;
    }


    [HttpPost]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Apply(
        ApplyApplicationRequest request)
    {
        var candidateId = User.FindFirst(
            ClaimTypes.NameIdentifier
        )?.Value;


        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return Unauthorized();
        }


        try
        {
            var result = await _service
                .ApplyAsync(candidateId, request);

            return Ok(result);
        }
        catch (ApplicationConflictException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }

        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }



    [HttpGet("vacancies/{vacancyId:guid}/apply-decision")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetApplyDecision(
        Guid vacancyId)
    {
        var candidateId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _service.GetApplyDecisionAsync(
                candidateId,
                vacancyId));
        }
        catch (ApplicationConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("candidate")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetMyApplications()
    {
        var candidateId = User.FindFirst(
            ClaimTypes.NameIdentifier
        )?.Value;


        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return Unauthorized();
        }


        var result = await _service
            .GetByCandidateAsync(candidateId);


        return Ok(result);
    }

    [HttpGet("candidate/{applicationId:guid}")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetMyApplication(
        Guid applicationId)
    {
        var candidateId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return Unauthorized();
        }

        var result = await _service.GetCandidateApplicationAsync(
            applicationId,
            candidateId);

        return result == null ? NotFound() : Ok(result);
    }



    [HttpGet("vacancy/{vacancyId}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetByVacancy(
        Guid vacancyId)
    {
        var employerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(employerId))
        {
            return Unauthorized();
        }

        var result = await _service
            .GetByVacancyAsync(vacancyId, employerId);

        return Ok(result);
    }





    [HttpGet("/api/employer/applications/{applicationId:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetEmployerApplication(
        Guid applicationId)
    {
        var employerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(employerId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _service.GetEmployerApplicationAsync(
                applicationId,
                employerId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("{applicationId}/status")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> UpdateStatus(
        Guid applicationId,
        [FromBody] JobApplicationDto request)
    {
        try
        {
            var employerId = User.FindFirst(
                ClaimTypes.NameIdentifier
            )?.Value;


            if (string.IsNullOrWhiteSpace(employerId))
            {
                return Unauthorized();
            }



            var application = await _repository
                .GetWithVacancyAsync(applicationId);



            if (application == null)
            {
                return NotFound(new
                {
                    message = "Application not found."
                });
            }




            if (application.Vacancy.EmployerId != employerId)
            {
                return Forbid();
            }





            var result = await _service
                .UpdateStatusAsync(
                    applicationId,
                    request.Status,
                    employerId);



            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("vacancy/{vacancyId}/compare")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> CompareCandidates(
        Guid vacancyId)
    {
        var employerId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(employerId))
        {
            return Unauthorized();
        }

        var candidates =
            await _service.GetByVacancyAsync(
                vacancyId,
                employerId);

        return Ok(candidates);
    }

    [HttpPut("{applicationId}/withdraw")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Withdraw(
        Guid applicationId)
    {
        var candidateId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return Unauthorized();
        }

        try
        {
            var application = await _service.WithdrawAsync(
                applicationId,
                candidateId);

            return Ok(application);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
