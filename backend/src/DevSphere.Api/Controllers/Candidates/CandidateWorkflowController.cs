using System.Security.Claims;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Candidates;

[ApiController]
[Route("api/candidate/applications")]
[Authorize(Roles = "Candidate")]
public class CandidateWorkflowController : ControllerBase
{
    private readonly IEmployerWorkflowService _workflowService;

    public CandidateWorkflowController(
        IEmployerWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("{applicationId:guid}/workflow")]
    public async Task<IActionResult> GetWorkflow(Guid applicationId)
    {
        var candidateId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _workflowService.GetCandidateSummaryAsync(
                candidateId,
                applicationId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
