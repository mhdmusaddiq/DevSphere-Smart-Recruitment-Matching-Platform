using DevSphere.Application.DTOs.Employers;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Employers;

[ApiController]
[Route("api/employer-architecture")]
[Authorize(Roles = "Employer")]
public class EmployerArchitectureController : ControllerBase
{
    private readonly IEmployerArchitectureService _service;

    public EmployerArchitectureController(IEmployerArchitectureService service)
    {
        _service = service;
    }

    [HttpPost("verifications")]
    public async Task<IActionResult> SubmitVerification(CompanyVerificationDto request, CancellationToken cancellationToken)
    {
        return Ok(await _service.SubmitVerificationAsync(request, cancellationToken));
    }

    [HttpPost("vacancy-requirements")]
    public async Task<IActionResult> AddRequirement(VacancyRequirementDto request, CancellationToken cancellationToken)
    {
        return Ok(await _service.AddRequirementAsync(request, cancellationToken));
    }

    [HttpPost("required-skills")]
    public async Task<IActionResult> AddRequiredSkill(RequiredSkillDto request, CancellationToken cancellationToken)
    {
        return Ok(await _service.AddRequiredSkillAsync(request, cancellationToken));
    }
}
