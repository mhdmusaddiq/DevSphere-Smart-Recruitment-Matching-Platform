using System.Security.Claims;
using DevSphere.Application.DTOs.Candidates;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Candidates;

[ApiController]
[Route("api/candidate-career")]
[Authorize(Roles = "Candidate")]
public class CandidateCareerController : ControllerBase
{
    private readonly ICandidateCareerService _service;

    public CandidateCareerController(ICandidateCareerService service)
    {
        _service = service;
    }

    private string? CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpGet("work-experiences")]
    public async Task<IActionResult> GetWorkExperiences(
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        return Ok(await _service.GetWorkExperiencesAsync(
            userId,
            cancellationToken));
    }

    [HttpPost("work-experiences")]
    public async Task<IActionResult> AddWorkExperience(
        WorkExperienceDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.AddWorkExperienceAsync(
                userId,
                request,
                cancellationToken);

            return result == null
                ? NotFound("Candidate profile not found.")
                : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("work-experiences/{id:guid}")]
    public async Task<IActionResult> UpdateWorkExperience(
        Guid id,
        WorkExperienceDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.UpdateWorkExperienceAsync(
                userId,
                id,
                request,
                cancellationToken);

            return result == null ? NotFound() : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("work-experiences/{id:guid}")]
    public async Task<IActionResult> DeleteWorkExperience(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var deleted = await _service.DeleteWorkExperienceAsync(
            userId,
            id,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("education")]
    public async Task<IActionResult> GetEducation(
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        return Ok(await _service.GetEducationAsync(
            userId,
            cancellationToken));
    }

    [HttpPost("education")]
    public async Task<IActionResult> AddEducation(
        EducationRecordDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.AddEducationAsync(
                userId,
                request,
                cancellationToken);

            return result == null
                ? NotFound("Candidate profile not found.")
                : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("education/{id:guid}")]
    public async Task<IActionResult> UpdateEducation(
        Guid id,
        EducationRecordDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.UpdateEducationAsync(
                userId,
                id,
                request,
                cancellationToken);

            return result == null ? NotFound() : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("education/{id:guid}")]
    public async Task<IActionResult> DeleteEducation(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var deleted = await _service.DeleteEducationAsync(
            userId,
            id,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("certifications")]
    public async Task<IActionResult> AddCertification(
        CertificationRecordDto request,
        CancellationToken cancellationToken) =>
        Ok(await _service.AddCertificationAsync(
            request,
            cancellationToken));

    [HttpPost("projects")]
    public async Task<IActionResult> AddProject(
        ProjectRecordDto request,
        CancellationToken cancellationToken) =>
        Ok(await _service.AddProjectAsync(
            request,
            cancellationToken));

    [HttpPost("languages")]
    public async Task<IActionResult> AddLanguage(
        LanguageCapabilityDto request,
        CancellationToken cancellationToken) =>
        Ok(await _service.AddLanguageAsync(
            request,
            cancellationToken));
}
