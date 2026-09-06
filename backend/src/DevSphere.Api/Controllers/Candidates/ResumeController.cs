using DevSphere.Application.DTOs.Candidates;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Candidates;

[ApiController]
[Route("api/candidates/{candidateProfileId:guid}/resume")]
[Authorize(Roles = "Candidate")]
public class ResumeController : ControllerBase
{
    private readonly IResumeService _service;

    public ResumeController(IResumeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid candidateProfileId, CancellationToken cancellationToken)
    {
        var resume = await _service.GetAsync(candidateProfileId, cancellationToken);
        return resume == null ? NotFound() : Ok(resume);
    }

    [HttpPost("versions")]
    public async Task<IActionResult> AddVersion(
        Guid candidateProfileId,
        ResumeVersionDto request,
        CancellationToken cancellationToken)
    {
        var resume = await _service.AddVersionAsync(candidateProfileId, request, cancellationToken);
        return Ok(resume);
    }
}
