using DevSphere.Application.DTOs.Application;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Applications;

[ApiController]
[Route("api/applications/{jobApplicationId:guid}")]
[Authorize]
public class ApplicationHistoryController : ControllerBase
{
    private readonly IApplicationHistoryService _service;

    public ApplicationHistoryController(IApplicationHistoryService service)
    {
        _service = service;
    }

    [HttpPost("snapshot")]
    public async Task<IActionResult> CreateSnapshot(
        Guid jobApplicationId,
        ApplicationSnapshotDto request,
        CancellationToken cancellationToken)
    {
        request.JobApplicationId = jobApplicationId;
        return Ok(await _service.CreateSnapshotAsync(request, cancellationToken));
    }

    [HttpGet("status-history")]
    public async Task<IActionResult> GetStatusHistory(Guid jobApplicationId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetStatusHistoryAsync(jobApplicationId, cancellationToken));
    }

    [HttpPost("status-history")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> RecordStatus(
        Guid jobApplicationId,
        ApplicationStatusHistoryDto request,
        CancellationToken cancellationToken)
    {
        request.JobApplicationId = jobApplicationId;
        return Ok(await _service.RecordStatusAsync(request, cancellationToken));
    }
}
