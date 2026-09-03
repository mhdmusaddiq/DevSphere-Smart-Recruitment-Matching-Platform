using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Auth;

[ApiController]
[Route("api/test")]
public class TestAuthController : ControllerBase
{
    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure()
    {
        return Ok(new
        {
            message = "JWT authentication working.",
            user = User.Identity?.Name
        });
    }
}
