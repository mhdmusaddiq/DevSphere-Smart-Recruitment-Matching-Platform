using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Profile;

[ApiController]
[Route("api/matching")]
public class MatchingController : ControllerBase
{
    private readonly IMatchEngine _engine;


    public MatchingController(
        IMatchEngine engine)
    {
        _engine = engine;
    }


    [HttpGet("{candidateId}/{vacancyId}")]
    [Authorize]
    public async Task<IActionResult> Calculate(
        string candidateId,
        string vacancyId)
    {
        try
        {
            var result = await _engine
                .CalculateAsync(
                    candidateId,
                    vacancyId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());

            return StatusCode(500, new
            {
                error = ex.Message,
                detail = ex.InnerException?.Message
            });
        }
    }
}