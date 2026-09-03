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
        var userId = User.FindFirst("sub")?.Value;

        var result = await _service
            .CreateAsync(userId!, request);

        return Ok(result);
    }
}