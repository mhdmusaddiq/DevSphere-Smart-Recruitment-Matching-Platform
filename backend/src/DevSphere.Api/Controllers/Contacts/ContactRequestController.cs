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


    [HttpPost]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Send(
        ContactRequestDto request)
    {
        var employerId = User.FindFirst(
            ClaimTypes.NameIdentifier
        )?.Value;


        if (string.IsNullOrWhiteSpace(employerId))
        {
            return Unauthorized();
        }


        request.EmployerId = employerId;


        var result = await _service.SendAsync(request);


        return Ok(result);
    }



    [HttpGet("candidate")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetCandidateRequests()
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



    [HttpGet("employer")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetEmployerRequests()
    {
        var employerId = User.FindFirst(
            ClaimTypes.NameIdentifier
        )?.Value;


        if (string.IsNullOrWhiteSpace(employerId))
        {
            return Unauthorized();
        }


        var result = await _service
            .GetByEmployerAsync(employerId);


        return Ok(result);
    }



    [HttpPut("{id}/status")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        ContactRequestDto request)
    {
        var result = await _service
            .UpdateStatusAsync(
                id,
                request.Status);


        return Ok(result);
    }
}