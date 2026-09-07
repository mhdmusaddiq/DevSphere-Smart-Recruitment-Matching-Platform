using DevSphere.Application.DTOs.Profile;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Profile;

[ApiController]
[Route("api/vacancies")]
public class VacancyController : ControllerBase
{
    private readonly IVacancyService _service;

    public VacancyController(
        IVacancyService service)
    {
        _service = service;
    }

    [HttpGet]
[AllowAnonymous]
public async Task<IActionResult> GetOpen(
    [FromQuery(Name = "q")] string? query,
    [FromQuery] string? location,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    if (page < 1)
    {
        page = 1;
    }

    if (pageSize < 1)
    {
        pageSize = 20;
    }

    if (pageSize > 100)
    {
        pageSize = 100;
    }

    var vacancies = await _service
        .GetOpenVacanciesAsync(
            query,
            location,
            page,
            pageSize);

    return Ok(vacancies);
}

    [HttpGet("{vacancyId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        Guid vacancyId)
    {
        var vacancy = await _service
            .GetByIdAsync(vacancyId);

        if (vacancy == null)
        {
            return NotFound();
        }

        return Ok(vacancy);
    }

    [HttpPost]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Create(
        VacancyDto request)
    {
        var employerId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        var result = await _service
            .CreateAsync(
                employerId!,
                request);

        return Ok(result);
    }

    [HttpPut("{vacancyId:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Update(
        Guid vacancyId,
        VacancyDto request)
    {
        var employerId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        var result = await _service
            .UpdateAsync(
                employerId!,
                vacancyId,
                request);

        return Ok(result);
    }

    [HttpPut("{vacancyId:guid}/close")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Close(
        Guid vacancyId)
    {
        var employerId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        var result = await _service
            .CloseAsync(
                employerId!,
                vacancyId);

        return Ok(result);
    }
}