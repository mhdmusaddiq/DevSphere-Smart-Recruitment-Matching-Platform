using DevSphere.Application.DTOs.Employers;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevSphere.Api.Controllers.Employers;

[ApiController]
[Route("api/companies")]
[Authorize(Roles = "Employer")]
public class CompanyTrustController : ControllerBase
{
    private readonly ICompanyTrustService _service;

    public CompanyTrustController(
        ICompanyTrustService service)
    {
        _service = service;
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var employerUserId = GetEmployerUserId();

        if (employerUserId == null)
        {
            return Unauthorized();
        }

        var companies = await _service
            .GetMineAsync(employerUserId);

        return Ok(companies);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCompanyProfileRequest request)
    {
        var employerUserId = GetEmployerUserId();

        if (employerUserId == null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _service
                .CreateCompanyAsync(
                    employerUserId,
                    request);

            return StatusCode(
                StatusCodes.Status201Created,
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{companyId:guid}/verification")]
    public async Task<IActionResult> SubmitVerification(
        Guid companyId,
        SubmitCompanyVerificationRequest request)
    {
        var employerUserId = GetEmployerUserId();

        if (employerUserId == null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _service
                .SubmitVerificationAsync(
                    employerUserId,
                    companyId,
                    request);

            return StatusCode(
                StatusCodes.Status201Created,
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message = ex.Message
                });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
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
    }

    private string? GetEmployerUserId()
    {
        return User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;
    }
}
