using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevSphere.Infrastructure.Identity;

namespace DevSphere.Api.Controllers.Test;

[ApiController]
[Route("api/test")]
public class RoleTestController : ControllerBase
{
    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet("admin")]
    public IActionResult Admin()
    {
        return Ok(new
        {
            message = "Admin access granted."
        });
    }


    [Authorize(Roles = AppRoles.Employer)]
    [HttpGet("employer")]
    public IActionResult Employer()
    {
        return Ok(new
        {
            message = "Employer access granted."
        });
    }


    [Authorize(Roles = AppRoles.Candidate)]
    [HttpGet("candidate")]
    public IActionResult Candidate()
    {
        return Ok(new
        {
            message = "Candidate access granted."
        });
    }
}