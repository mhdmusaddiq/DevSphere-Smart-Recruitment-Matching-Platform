using System.Security.Claims;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Candidates;

[ApiController]
[Route("api/candidates/resume")]
[Authorize(Roles = "Candidate")]
public class ResumeController : ControllerBase
{
    private readonly IResumeService _service;
    private readonly IFileValidationService _validation;

    public ResumeController(
        IResumeService service,
        IFileValidationService validation)
    {
        _service = service;
        _validation = validation;
    }

    private string? CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var resume = await _service.GetOwnAsync(
            userId,
            cancellationToken);

        return resume == null
            ? NotFound()
            : Ok(resume);
    }

    [HttpPost("versions")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadVersion(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (file == null)
        {
            return BadRequest(new
            {
                message = "A CV file is required."
            });
        }

        var validation = _validation.Validate(
            file.FileName,
            file.ContentType,
            file.Length);

        if (!validation.IsValid)
        {
            var response = new
            {
                message = validation.Error
            };

            return validation.TooLarge
                ? StatusCode(
                    StatusCodes.Status413PayloadTooLarge,
                    response)
                : BadRequest(response);
        }

        await using var source = file.OpenReadStream();
        await using var stream = await _validation.BufferAsync(
            source,
            cancellationToken);

        var contentValidation = await _validation.ValidatePdfContentAsync(
            stream,
            cancellationToken);

        if (!contentValidation.IsValid)
        {
            return BadRequest(new
            {
                message = contentValidation.Error
            });
        }

        stream.Position = 0;

        var resume = await _service.AddUploadedVersionAsync(
            userId,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);

        if (resume == null)
        {
            return NotFound(new
            {
                message = "Candidate profile not found."
            });
        }

        return Ok(resume);
    }

    [HttpGet("versions/{versionId:guid}/download")]
    public async Task<IActionResult> DownloadVersion(
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var download = await _service.DownloadOwnVersionAsync(
            userId,
            versionId,
            cancellationToken);

        if (download == null)
        {
            return NotFound();
        }

        return File(
            download.Content,
            download.ContentType,
            download.FileName,
            enableRangeProcessing: true);
    }

    [HttpPut("versions/{versionId:guid}/current")]
    public async Task<IActionResult> SetCurrentVersion(
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var resume = await _service.SetCurrentVersionAsync(
            userId,
            versionId,
            cancellationToken);

        return resume == null
            ? NotFound()
            : Ok(resume);
    }
}
