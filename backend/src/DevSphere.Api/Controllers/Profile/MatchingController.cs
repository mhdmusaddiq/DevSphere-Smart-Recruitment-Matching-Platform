using System.Security.Claims;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Profile;

[ApiController]
[Route("api/matching")]
[Authorize(Roles = "Candidate")]
public class MatchingController : ControllerBase
{
    private readonly IMatchEngine _engine;

    public MatchingController(IMatchEngine engine)
    {
        _engine = engine;
    }

    [HttpGet("vacancies/{vacancyId}")]
    public async Task<IActionResult> Calculate(string vacancyId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _engine.CalculateAsync(userId, vacancyId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
