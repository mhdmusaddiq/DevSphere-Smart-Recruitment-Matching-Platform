using DevSphere.Application.DTOs.Employers;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevSphere.Api.Controllers.Employers;

[ApiController]
[Route("api/employer-workflow")]
[Authorize(Roles = "Employer")]
public class EmployerWorkflowController :
    ControllerBase
{
    private readonly IEmployerWorkflowService _service;

    public EmployerWorkflowController(
        IEmployerWorkflowService service)
    {
        _service = service;
    }

    [HttpPost("interviews")]
    public async Task<IActionResult> CreateInterview(
        CreateInterviewRequest request)
    {
        return await ExecuteAsync(
            () => _service.CreateInterviewAsync(
                GetEmployerUserId(),
                request),
            true);
    }

    [HttpPut("interviews/{interviewId:guid}/status")]
    public async Task<IActionResult> UpdateInterviewStatus(
        Guid interviewId,
        UpdateInterviewStatusRequest request)
    {
        return await ExecuteAsync(async () =>
            await _service.UpdateInterviewStatusAsync(
                GetEmployerUserId(),
                interviewId,
                request));
    }
    [HttpPost("interviews/{interviewId:guid}/slots")]
    public async Task<IActionResult> AddInterviewSlot(
        Guid interviewId,
        CreateInterviewSlotRequest request)
    {
        return await ExecuteAsync(
            () => _service.AddInterviewSlotAsync(
                GetEmployerUserId(),
                interviewId,
                request),
            true);
    }

    [HttpPost("scorecards")]
    public async Task<IActionResult> CreateScorecard(
        CreateScorecardRequest request)
    {
        return await ExecuteAsync(
            () => _service.CreateScorecardAsync(
                GetEmployerUserId(),
                request),
            true);
    }

    [HttpPost("offers")]
    public async Task<IActionResult> CreateOffer(
        CreateOfferRequest request)
    {
        return await ExecuteAsync(
            () => _service.CreateOfferAsync(
                GetEmployerUserId(),
                request),
            true);
    }

    [HttpPut("offers/{offerId:guid}/status")]
    public async Task<IActionResult> UpdateOfferStatus(
        Guid offerId,
        UpdateOfferStatusRequest request)
    {
        return await ExecuteAsync(
            () => _service.UpdateOfferStatusAsync(
                GetEmployerUserId(),
                offerId,
                request));
    }

    [HttpPost("talent-pool")]
    public async Task<IActionResult> AddTalentPoolEntry(
        CreateTalentPoolEntryRequest request)
    {
        return await ExecuteAsync(
            () => _service.AddTalentPoolEntryAsync(
                GetEmployerUserId(),
                request),
            true);
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
