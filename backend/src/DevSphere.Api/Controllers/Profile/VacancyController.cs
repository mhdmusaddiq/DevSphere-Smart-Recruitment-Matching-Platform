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
    public async Task<IActionResult> GetOpen()
    {
        var vacancies = await _service
            .GetOpenVacanciesAsync();

        return Ok(vacancies);
    }


    [HttpPost]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Create(
        VacancyDto request)
    {
        var employerId = User
            .FindFirst("sub")?
            .Value;

        var result = await _service
            .CreateAsync(
                employerId!,
                request);

        return Ok(result);
    }
}