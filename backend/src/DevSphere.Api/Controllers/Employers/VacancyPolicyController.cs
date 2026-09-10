using DevSphere.Application.DTOs.Employers;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevSphere.Api.Controllers.Employers;

[ApiController]
[Route("api/vacancies/{vacancyId:guid}/policy")]
[Authorize(Roles = "Employer")]
public class VacancyPolicyController :
    ControllerBase
{
    private readonly IVacancyPolicyService _service;

    public VacancyPolicyController(
        IVacancyPolicyService service)
    {
        _service = service;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(
        Guid vacancyId)
    {
        return await ExecuteAsync(
            () => _service.GetCurrentRevisionAsync(
                GetEmployerUserId(),
                vacancyId));
    }

    [HttpGet("current/full")]
    public async Task<IActionResult> GetCurrentFull(
        Guid vacancyId)
    {
        return await ExecuteAsync(
            () => _service.GetCurrentAggregateAsync(
                GetEmployerUserId(),
                vacancyId));
    }

    [HttpPut("current")]
    public async Task<IActionResult> ReplaceCurrent(
        Guid vacancyId,
        VacancyPolicyAggregateUpdateRequest request)
    {
        return await ExecuteAsync(
            () => _service.ReplaceCurrentAggregateAsync(
                GetEmployerUserId(),
                vacancyId,
                request));
    }

    [HttpPost("families")]
    public async Task<IActionResult> AddFamily(
        Guid vacancyId,
        FamilyPolicyRequest request)
    {
        return await ExecuteAsync(
            () => _service.AddFamilyPolicyAsync(
                GetEmployerUserId(),
                vacancyId,
                request),
            created: true);
    }

    [HttpPost("requirements")]
    public async Task<IActionResult> AddRequirement(
        Guid vacancyId,
        VacancyRequirementRequest request)
    {
        return await ExecuteAsync(
            () => _service.AddRequirementAsync(
                GetEmployerUserId(),
                vacancyId,
                request),
            created: true);
    }

    [HttpPost("alternative-sets")]
    public async Task<IActionResult> AddAlternativeSet(
        Guid vacancyId,
        AlternativeSetRequest request)
    {
        return await ExecuteAsync(
            () => _service.AddAlternativeSetAsync(
                GetEmployerUserId(),
                vacancyId,
                request),
            created: true);
    }

    private string GetEmployerUserId()
    {
        return User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
    }

    private async Task<IActionResult> ExecuteAsync<T>(
        Func<Task<T>> action,
        bool created = false)
    {
        try
        {
            var result = await action();

            return created
                ? StatusCode(
                    StatusCodes.Status201Created,
                    result)
                : Ok(result);
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
}
