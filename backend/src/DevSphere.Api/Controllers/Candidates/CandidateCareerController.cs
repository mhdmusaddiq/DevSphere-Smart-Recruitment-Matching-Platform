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

    [HttpGet("work-experiences/{candidateProfileId:guid}")]
    public async Task<IActionResult> GetWorkExperiences(
        Guid candidateProfileId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetWorkExperiencesAsync(
            candidateProfileId,
            cancellationToken));

    [HttpPost("work-experiences")]
    public async Task<IActionResult> AddWorkExperience(
        WorkExperienceDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.AddWorkExperienceAsync(
                request,
                cancellationToken));
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
        try
        {
            var result = await _service.UpdateWorkExperienceAsync(
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
        var deleted = await _service.DeleteWorkExperienceAsync(
            id,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("education/{candidateProfileId:guid}")]
    public async Task<IActionResult> GetEducation(
        Guid candidateProfileId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetEducationAsync(
            candidateProfileId,
            cancellationToken));

    [HttpPost("education")]
    public async Task<IActionResult> AddEducation(
        EducationRecordDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.AddEducationAsync(
                request,
                cancellationToken));
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
        try
        {
            var result = await _service.UpdateEducationAsync(
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
        var deleted = await _service.DeleteEducationAsync(
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
