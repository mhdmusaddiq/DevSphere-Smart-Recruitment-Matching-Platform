using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevSphere.Api.Controllers.Dashboards;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet("candidate")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetCandidate(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrWhiteSpace(userId)
            ? Unauthorized()
            : Ok(await _service.GetCandidateAsync(userId, cancellationToken));
    }

    [HttpGet("employer")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetEmployer(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrWhiteSpace(userId)
            ? Unauthorized()
            : Ok(await _service.GetEmployerAsync(userId, cancellationToken));
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdmin(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAdminAsync(cancellationToken));
    }
}
