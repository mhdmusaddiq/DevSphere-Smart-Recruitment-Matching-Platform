using DevSphere.Application.DTOs.Profile;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Profile;

[ApiController]
[Route("api/profile/candidate")]
[Authorize(Roles = "Candidate")]
public class CandidateProfileController : ControllerBase
{
    private readonly ICandidateProfileService _service;

    public CandidateProfileController(
        ICandidateProfileService service)
    {
        _service = service;
    }


    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirst(
     System.Security.Claims.ClaimTypes.NameIdentifier
 )?.Value;

        var profile = await _service
            .GetByUserIdAsync(userId!);

        return Ok(profile);
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        CandidateProfileDto request)
    {
        var userId = User.FindFirst(
     System.Security.Claims.ClaimTypes.NameIdentifier
 )?.Value;

        var result = await _service
            .CreateAsync(userId!, request);

        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        CandidateProfileDto request)
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        var result = await _service.UpdateAsync(userId!, request);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
    [HttpGet("skills")]
    public async Task<IActionResult> GetSkills()
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        var result = await _service.GetSkillsAsync(userId!);

        return Ok(result);
    }

    [HttpPost("skills")]
    public async Task<IActionResult> AddSkill(
        AddCandidateSkillDto request)
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        try
        {
            var result = await _service
                .AddSkillAsync(userId!, request);

            if (result == null)
            {
                return NotFound("Candidate profile not found.");
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("skills/{skillId:guid}")]
    public async Task<IActionResult> DeleteSkill(
        Guid skillId)
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        var deleted = await _service
            .DeleteSkillAsync(userId!, skillId);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("readiness")]
    public async Task<IActionResult> GetReadiness()
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var result = await _service
            .GetProfileReadinessAsync(userId);

        return Ok(result);
    }

    [HttpGet("application-readiness")]
    public async Task<IActionResult> GetApplicationReadiness()
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var result = await _service
            .GetApplicationReadinessAsync(userId);

        return Ok(result);
    }
}
