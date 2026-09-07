using DevSphere.Application.DTOs.Contacts;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevSphere.Api.Controllers.Contacts;

[ApiController]
[Route("api/contact-requests")]
public class ContactRequestController : ControllerBase
{
    private readonly IContactRequestService _service;


    public ContactRequestController(
        IContactRequestService service)
    {
        _service = service;
    }


    [HttpPost("applications/{jobApplicationId:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Send(
        Guid jobApplicationId)
    {
        var employerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(employerId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _service.SendAsync(
                jobApplicationId,
                employerId);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }


    [HttpGet("candidate")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetCandidateRequests()
    {
        var candidateId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return Unauthorized();
        }

        var result = await _service
            .GetByCandidateAsync(candidateId);

        return Ok(result);
    }


    [HttpGet("employer")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetEmployerRequests()
    {
        var employerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(employerId))
        {
            return Unauthorized();
        }

        var result = await _service
            .GetByEmployerAsync(employerId);

        return Ok(result);
    }


    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        ContactRequestDto request)
    {
        var candidateId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _service
                .UpdateStatusAsync(
                    id,
                    request.Status,
                    candidateId);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
