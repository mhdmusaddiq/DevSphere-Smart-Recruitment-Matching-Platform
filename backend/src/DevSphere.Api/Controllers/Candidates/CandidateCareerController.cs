using DevSphere.Application.DTOs.Candidates;
using DevSphere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevSphere.Api.Controllers.Candidates;

[ApiController]
[Route("api/candidate-career")]
[Authorize(Roles = "Candidate")]
public class CandidateCareerController : ControllerBase
{
    private readonly ICandidateCareerService _service;

    public CandidateCareerController(ICandidateCareerService service)
    {
        _service = service;
    }

    [HttpPost("work-experiences")]
    public async Task<IActionResult> AddWorkExperience(WorkExperienceDto request, CancellationToken cancellationToken) =>
        Ok(await _service.AddWorkExperienceAsync(request, cancellationToken));

    [HttpPost("education")]
    public async Task<IActionResult> AddEducation(EducationRecordDto request, CancellationToken cancellationToken) =>
        Ok(await _service.AddEducationAsync(request, cancellationToken));

    [HttpPost("certifications")]
    public async Task<IActionResult> AddCertification(CertificationRecordDto request, CancellationToken cancellationToken) =>
        Ok(await _service.AddCertificationAsync(request, cancellationToken));

    [HttpPost("projects")]
    public async Task<IActionResult> AddProject(ProjectRecordDto request, CancellationToken cancellationToken) =>
        Ok(await _service.AddProjectAsync(request, cancellationToken));

    [HttpPost("languages")]
    public async Task<IActionResult> AddLanguage(LanguageCapabilityDto request, CancellationToken cancellationToken) =>
        Ok(await _service.AddLanguageAsync(request, cancellationToken));
}
