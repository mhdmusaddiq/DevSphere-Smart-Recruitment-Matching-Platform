using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevSphere.Api.Controllers.Profile;

[ApiController]
[Route("api/applications")]
public class JobApplicationController : ControllerBase
{
    private readonly IJobApplicationService _service;

    public JobApplicationController(
        IJobApplicationService service)
    {
        _service = service;
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
            var result = await _service.ApplyAsync(request);

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