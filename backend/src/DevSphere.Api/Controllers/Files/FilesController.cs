using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Files;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _storage;
    private readonly IFileValidationService _validation;

    public FilesController(IFileStorageService storage, IFileValidationService validation)
    {
        _storage = storage;
        _validation = validation;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        var validation = _validation.Validate(file.FileName, file.ContentType, file.Length);
        if (!validation.IsValid)
        {
            return BadRequest(new { message = validation.Error });
        }

        await using var stream = file.OpenReadStream();
        var stored = await _storage.SaveAsync(
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);
        return Ok(stored);
    }

    [HttpGet("{storageKey}")]
    public async Task<IActionResult> Download(string storageKey, CancellationToken cancellationToken)
    {
        var stream = await _storage.OpenReadAsync(storageKey, cancellationToken);
        return File(stream, "application/octet-stream", enableRangeProcessing: true);
    }
}
