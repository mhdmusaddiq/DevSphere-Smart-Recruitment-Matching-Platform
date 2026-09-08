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

    [HttpGet("certifications")]
    public async Task<IActionResult> GetCertifications(
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        return Ok(await _service.GetCertificationsAsync(
            userId,
            cancellationToken));
    }

    [HttpPost("certifications")]
    public async Task<IActionResult> AddCertification(
        CertificationRecordDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.AddCertificationAsync(
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

    [HttpPut("certifications/{id:guid}")]
    public async Task<IActionResult> UpdateCertification(
        Guid id,
        CertificationRecordDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.UpdateCertificationAsync(
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

    [HttpDelete("certifications/{id:guid}")]
    public async Task<IActionResult> DeleteCertification(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var deleted = await _service.DeleteCertificationAsync(
            userId,
            id,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("projects")]
    public async Task<IActionResult> GetProjects(
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        return Ok(await _service.GetProjectsAsync(
            userId,
            cancellationToken));
    }

    [HttpPost("projects")]
    public async Task<IActionResult> AddProject(
        ProjectRecordDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.AddProjectAsync(
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

    [HttpPut("projects/{id:guid}")]
    public async Task<IActionResult> UpdateProject(
        Guid id,
        ProjectRecordDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.UpdateProjectAsync(
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

    [HttpDelete("projects/{id:guid}")]
    public async Task<IActionResult> DeleteProject(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var deleted = await _service.DeleteProjectAsync(
            userId,
            id,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages(
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        return Ok(await _service.GetLanguagesAsync(
            userId,
            cancellationToken));
    }

    [HttpPost("languages")]
    public async Task<IActionResult> AddLanguage(
        LanguageCapabilityDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.AddLanguageAsync(
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

    [HttpPut("languages/{id:guid}")]
    public async Task<IActionResult> UpdateLanguage(
        Guid id,
        LanguageCapabilityDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.UpdateLanguageAsync(
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

    [HttpDelete("languages/{id:guid}")]
    public async Task<IActionResult> DeleteLanguage(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var deleted = await _service.DeleteLanguageAsync(
            userId,
            id,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("licences")]
    public async Task<IActionResult> GetLicences(
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        return Ok(await _service.GetLicencesAsync(
            userId,
            cancellationToken));
    }

    [HttpPost("licences")]
    public async Task<IActionResult> AddLicence(
        LicenceRegistrationDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.AddLicenceAsync(
                userId,
                request,
                cancellationToken);

            return result == null
                ? NotFound(new
                {
                    message = "Candidate profile not found."
                })
                : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("licences/{id:guid}")]
    public async Task<IActionResult> UpdateLicence(
        Guid id,
        LicenceRegistrationDto request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var result = await _service.UpdateLicenceAsync(
                userId,
                id,
                request,
                cancellationToken);

            return result == null
                ? NotFound()
                : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("licences/{id:guid}")]
    public async Task<IActionResult> DeleteLicence(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        return await _service.DeleteLicenceAsync(
            userId,
            id,
            cancellationToken)
                ? NoContent()
                : NotFound();
    }
}
