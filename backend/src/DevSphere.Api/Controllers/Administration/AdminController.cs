using System.Security.Claims;
using DevSphere.Application.DTOs.Administration;
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
    public async Task<IActionResult> GetAuditEvents(
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        return Ok(
            await _service.GetRecentAuditEventsAsync(
                take,
                cancellationToken));
    }

    [HttpGet("system-settings")]
    public async Task<IActionResult> GetSystemSettings(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetSystemSettingsAsync(
                cancellationToken));
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        return Ok(
            await _service.GetUsersAsync(
                page,
                pageSize,
                cancellationToken));
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        CancellationToken cancellationToken = default)
    {
        return Ok(
            await _service.GetAccountDashboardAsync(
                cancellationToken));
    }

    [HttpPut("users/{userId}/status")]
    public async Task<IActionResult> SetAccountStatus(
        string userId,
        UpdateAccountStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var actorUserId = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _service
                .SetAccountStatusAsync(
                    actorUserId,
                    userId,
                    request.IsActive,
                    cancellationToken);

            return result == null
                ? NotFound()
                : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }
}
