using DevSphere.Infrastructure.Configurations;
using DevSphere.Infrastructure.Services.Files;
using Microsoft.Extensions.Options;

namespace DevSphere.Tests.Files;

public class FileValidationServiceTests
{
    private static FileValidationService CreateService()
    {
        var options = Options.Create(new FileStorageOptions
        {
            MaximumFileSizeBytes = 10 * 1024 * 1024,
            AllowedExtensions = [".pdf", ".doc", ".docx"],
            AllowedContentTypes =
            [
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            ]
        });

        return new FileValidationService(options);
    }

    [Fact]
    public void Validate_Should_Accept_Valid_Pdf()
    {
        var service = CreateService();

        var result = service.Validate(
            "candidate-cv.pdf",
            "application/pdf",
            1024);

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Validate_Should_Reject_Disallowed_Extension()
    {
        var service = CreateService();

        var result = service.Validate(
            "candidate-cv.exe",
            "application/pdf",
            1024);

        Assert.False(result.IsValid);
        Assert.Equal(
            "The file extension is not allowed.",
            result.Error);
    }

    [Fact]
    public void Validate_Should_Reject_Disallowed_ContentType()
    {
        var service = CreateService();

        var result = service.Validate(
            "candidate-cv.pdf",
            "application/x-msdownload",
            1024);

        Assert.False(result.IsValid);
        Assert.Equal(
            "The file content type is not allowed.",
            result.Error);
    }

    [Fact]
    public void Validate_Should_Reject_Empty_File()
    {
        var service = CreateService();

        var result = service.Validate(
            "candidate-cv.pdf",
            "application/pdf",
            0);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Should_Reject_File_Over_Size_Limit()
    {
        var service = CreateService();

        var result = service.Validate(
            "candidate-cv.pdf",
            "application/pdf",
            (10 * 1024 * 1024) + 1);

        Assert.False(result.IsValid);
    }
}
