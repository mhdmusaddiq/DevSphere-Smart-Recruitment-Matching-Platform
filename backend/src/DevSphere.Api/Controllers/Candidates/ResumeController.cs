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
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadVersion(
        [FromForm] IFormFile file,
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

        await using var stream = file.OpenReadStream();

        var signature = new byte[5];
        var bytesRead = await stream.ReadAsync(
            signature,
            cancellationToken);
        stream.Position = 0;

        if (bytesRead != signature.Length ||
            !signature.SequenceEqual("%PDF-"u8.ToArray()))
        {
            return BadRequest(new
            {
                message = "The uploaded file is not a valid PDF."
            });
        }

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
