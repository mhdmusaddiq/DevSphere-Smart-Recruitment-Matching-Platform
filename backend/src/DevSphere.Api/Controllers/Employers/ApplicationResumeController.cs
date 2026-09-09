using System.Security.Claims;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Employers;

[ApiController]
[Route("api/employer/applications")]
[Authorize(Roles = "Employer")]
public class ApplicationResumeController : ControllerBase
{
    private readonly IResumeService _resumeService;

    public ApplicationResumeController(IResumeService resumeService)
    {
        _resumeService = resumeService;
    }

    [HttpGet("{applicationId:guid}/resume")]
    public async Task<IActionResult> Download(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var employerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(employerId))
        {
            return Unauthorized();
        }

        var download = await _resumeService
            .DownloadApplicationVersionAsync(
                employerId,
                applicationId,
                cancellationToken);

        return download == null
            ? NotFound()
            : File(
                download.Content,
                download.ContentType,
                download.FileName,
                enableRangeProcessing: true);
    }
}
