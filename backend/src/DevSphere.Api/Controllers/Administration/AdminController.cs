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

    [HttpGet("catalogue/skills")]
    public async Task<IActionResult> GetSkillCatalogue(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(
            await _service.GetSkillConceptsAsync(
                includeInactive,
                cancellationToken));
    }

    [HttpGet("catalogue/aliases")]
    public async Task<IActionResult> GetSkillAliases(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(
            await _service.GetSkillAliasesAsync(
                includeInactive,
                cancellationToken));
    }

    [HttpGet("catalogue/occupations")]
    public async Task<IActionResult> GetOccupationCatalogue(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(
            await _service.GetOccupationConceptsAsync(
                includeInactive,
                cancellationToken));
    }

    [HttpPut("catalogue/skills/{conceptId:guid}/status")]
    public async Task<IActionResult> SetSkillConceptStatus(
        Guid conceptId,
        UpdateCatalogueStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var result =
            await _service.SetSkillConceptStatusAsync(
                conceptId,
                request.IsActive,
                cancellationToken);

        return result == null
            ? NotFound()
            : Ok(result);
    }

    [HttpPut("catalogue/occupations/{occupationId:guid}/status")]
    public async Task<IActionResult> SetOccupationConceptStatus(
        Guid occupationId,
        UpdateCatalogueStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var result =
            await _service.SetOccupationConceptStatusAsync(
                occupationId,
                request.IsActive,
                cancellationToken);

        return result == null
            ? NotFound()
            : Ok(result);
    }
    [HttpGet("company-verifications")]
    public async Task<IActionResult>
        GetCompanyVerifications(
            [FromQuery] string? status = null,
            CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(
                await _service
                    .GetCompanyVerificationsAsync(
                        status,
                        cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("company-verifications/{verificationId:guid}")]
    public async Task<IActionResult>
        GetCompanyVerification(
            Guid verificationId,
            CancellationToken cancellationToken = default)
    {
        var result =
            await _service
                .GetCompanyVerificationAsync(
                    verificationId,
                    cancellationToken);

        return result == null
            ? NotFound()
            : Ok(result);
    }

    [HttpPut("company-verifications/{verificationId:guid}/review")]
    public async Task<IActionResult>
        ReviewCompanyVerification(
            Guid verificationId,
            ReviewCompanyVerificationRequest request,
            CancellationToken cancellationToken = default)
    {
        var actorUserId =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(
            actorUserId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _service
                    .ReviewCompanyVerificationAsync(
                        actorUserId,
                        verificationId,
                        request.Decision,
                        cancellationToken);

            return result == null
                ? NotFound()
                : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }}
