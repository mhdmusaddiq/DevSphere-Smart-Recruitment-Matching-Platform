using DevSphere.Application.DTOs.Application;
using DevSphere.Infrastructure.Services.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Profile;

[ApiController]
[Route("api/applications")]
public class JobApplicationController : ControllerBase
{
    private readonly JobApplicationService _service;

    public JobApplicationController(
        JobApplicationService service)
    {
        _service = service;
    }


    [HttpPost]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Apply(
        JobApplicationDto request)
    {
        var result = await _service
            .ApplyAsync(request);

        return Ok(result);
    }


    [HttpGet("candidate")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetMyApplications()
    {
        var candidateId = User
            .FindFirst("sub")?
            .Value;

        var result = await _service
            .GetByCandidateAsync(candidateId!);

        return Ok(result);
    }
}
