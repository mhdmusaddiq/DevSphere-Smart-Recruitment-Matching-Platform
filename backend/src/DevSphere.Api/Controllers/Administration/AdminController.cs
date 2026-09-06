using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Administration;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _service;

    public AdminController(IAdminService service)
    {
        _service = service;
    }

    [HttpGet("audit-events")]
    public async Task<IActionResult> GetAuditEvents([FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        return Ok(await _service.GetRecentAuditEventsAsync(take, cancellationToken));
    }

    [HttpGet("system-settings")]
    public async Task<IActionResult> GetSystemSettings(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSystemSettingsAsync(cancellationToken));
    }
}
