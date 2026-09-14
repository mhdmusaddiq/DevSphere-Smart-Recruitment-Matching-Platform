using DevSphere.Application.DTOs.Candidates;
using DevSphere.Infrastructure.Services.Candidates;

namespace DevSphere.Tests.Candidates;

public class CandidateCareerServiceTests
{
    [Fact]
    public async Task AddWorkExperience_Should_Reject_EndDate_Before_StartDate()
    {
        var service = new CandidateCareerService(
            null!,
            null!);

        var request = new WorkExperienceDto
        {
            JobTitle = "Software Engineer",
            CompanyName = "DevSphere",
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 8, 1)
        };

        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.AddWorkExperienceAsync(
                    "candidate-user",
                    request));

        Assert.Equal(
            "End date cannot be earlier than start date.",
            exception.Message);
    }

    [Fact]
    public async Task AddWorkExperience_Should_Reject_Blank_JobTitle()
    {
        var service = new CandidateCareerService(
            null!,
            null!);

        var request = new WorkExperienceDto
        {
            JobTitle = " ",
            CompanyName = "DevSphere",
            StartDate = new DateOnly(2026, 1, 1)
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddWorkExperienceAsync(
                "candidate-user",
                request));
    }

    [Fact]
    public async Task AddEducation_Should_Reject_Invalid_Date_Period()
    {
        var service = new CandidateCareerService(
            null!,
            null!);

        var request = new EducationRecordDto
        {
            Institution = "University",
            Qualification = "BSc",
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2024, 1, 1)
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddEducationAsync(
                "candidate-user",
                request));
    }

    [Fact]
    public async Task AddLicence_Should_Reject_Expiry_Before_Issue()
    {
        var service = new CandidateCareerService(
            null!,
            null!);

        var request = new LicenceRegistrationDto
        {
            Type = "Professional",
            Issuer = "DevSphere Authority",
            Identifier = "LIC-001",
            IssuedOn = new DateOnly(2026, 9, 1),
            ExpiresOn = new DateOnly(2026, 8, 1)
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddLicenceAsync(
                "candidate-user",
                request));
    }

    [Fact]
    public async Task AddLicence_Should_Reject_Blank_Identifier()
    {
        var service = new CandidateCareerService(
            null!,
            null!);

        var request = new LicenceRegistrationDto
        {
            Type = "Professional",
            Issuer = "DevSphere Authority",
            Identifier = " ",
            IssuedOn = new DateOnly(2026, 9, 1)
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddLicenceAsync(
                "candidate-user",
                request));
    }}
