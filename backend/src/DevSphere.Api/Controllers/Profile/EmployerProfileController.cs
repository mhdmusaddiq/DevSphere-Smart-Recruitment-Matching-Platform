using DevSphere.Application.DTOs.Profile;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Profile;

[ApiController]
[Route("api/profile/employer")]
[Authorize(Roles = "Employer")]
public class EmployerProfileController : ControllerBase
{
    private readonly IEmployerProfileService _service;

    public EmployerProfileController(
        IEmployerProfileService service)
    {
        _service = service;
    }


    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirst("sub")?.Value;

        var profile = await _service
            .GetByUserIdAsync(userId!);

        return Ok(profile);
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        EmployerProfileDto request)
    {
        var userId = User.FindFirst("sub")?.Value;

        var result = await _service
            .CreateAsync(userId!, request);

        return Ok(result);
    }
}