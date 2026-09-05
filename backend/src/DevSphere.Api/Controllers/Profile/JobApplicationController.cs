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
        JobApplicationDto request)
    {
        var candidateId = User.FindFirst(
            ClaimTypes.NameIdentifier
        )?.Value;


        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return Unauthorized();
        }


        request.CandidateId = candidateId;


        try
        {
            var result = await _service
                .ApplyAsync(request);

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




    [HttpGet("vacancy/{vacancyId}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetByVacancy(
        string vacancyId)
    {
        var result = await _service
            .GetByVacancyAsync(vacancyId);


        return Ok(result);
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
                    request.Status);



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
}