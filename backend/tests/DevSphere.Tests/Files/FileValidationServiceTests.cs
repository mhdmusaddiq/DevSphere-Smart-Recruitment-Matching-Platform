using DevSphere.Infrastructure.Configurations;
using DevSphere.Infrastructure.Services.Files;
using Microsoft.Extensions.Options;
using System.Text;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace DevSphere.Tests.Files;

public class FileValidationServiceTests
{
    private static FileValidationService CreateService()
    {
        var options = Options.Create(new FileStorageOptions());

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
            (5 * 1024 * 1024) + 1);

        Assert.False(result.IsValid);
        Assert.True(result.TooLarge);
    }

    [Fact]
    public void Validate_Should_Reject_Word_Cv()
    {
        var service = CreateService();

        var result = service.Validate(
            "candidate-cv.docx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            1024);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidatePdfContentAsync_Should_Accept_Structurally_Valid_Pdf_And_Reset_Stream()
    {
        var service = CreateService();
        await using var stream = new MemoryStream(CreateValidPdf());

        var result = await service.ValidatePdfContentAsync(stream, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public async Task ValidatePdfContentAsync_Should_Accept_Classic_Xref_Pdf()
    {
        var service = CreateService();
        await using var stream = new MemoryStream(CreateClassicXrefPdf());

        var result = await service.ValidatePdfContentAsync(stream, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public async Task ValidatePdfContentAsync_Should_Reject_Garbage_With_Pdf_Header()
    {
        var service = CreateService();
        await using var stream = new MemoryStream(
            "%PDF-1.7\nThis is text, not a PDF.\nstartxref\n0\n%%EOF"u8.ToArray());

        var result = await service.ValidatePdfContentAsync(stream, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Equal("The uploaded file is not a valid PDF.", result.Error);
    }

    [Fact]
    public async Task ValidatePdfContentAsync_Should_Reject_Truncated_Pdf()
    {
        var service = CreateService();
        var valid = CreateValidPdf();
        await using var stream = new MemoryStream(valid[..(valid.Length / 2)]);

        var result = await service.ValidatePdfContentAsync(stream, CancellationToken.None);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidatePdfContentAsync_Should_Reject_Malformed_Structure()
    {
        var service = CreateService();
        var malformed = """
            %PDF-1.7
            1 0 obj
            << /Type /Catalog /Pages 99 0 R >>
            endobj
            startxref
            0
            %%EOF
            """;
        await using var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(malformed));

        var result = await service.ValidatePdfContentAsync(stream, CancellationToken.None);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task BufferAsync_Should_Preserve_NonSeekable_Content_For_Validation_And_Storage()
    {
        var service = CreateService();
        var expected = CreateValidPdf();
        await using var source = new NonSeekableReadStream(expected);
        await using var buffered = await service.BufferAsync(source, CancellationToken.None);

        var validation = await service.ValidatePdfContentAsync(buffered, CancellationToken.None);
        Assert.True(validation.IsValid);
        Assert.Equal(0, buffered.Position);

        using var actual = new MemoryStream();
        await buffered.CopyToAsync(actual);
        Assert.Equal(expected, actual.ToArray());
    }

    [Fact]
    public async Task NonSeekable_Pdf_Should_Be_Stored_Byte_For_Byte_After_Validation()
    {
        var rootPath = Path.Combine(
            Path.GetTempPath(),
            "devsphere-nonseekable-tests",
            Guid.NewGuid().ToString("N"));
        var options = Options.Create(new FileStorageOptions { RootPath = rootPath });
        var validationService = new FileValidationService(options);
        var storageService = new LocalFileStorageService(options);
        var expected = CreateValidPdf();

        try
        {
            await using var source = new NonSeekableReadStream(expected);
            await using var buffered = await validationService.BufferAsync(source, CancellationToken.None);
            var validation = await validationService.ValidatePdfContentAsync(buffered, CancellationToken.None);
            Assert.True(validation.IsValid);

            var stored = await storageService.SaveAsync(
                buffered,
                "candidate.pdf",
                "application/pdf",
                expected.LongLength,
                CancellationToken.None);
            await using var opened = await storageService.OpenReadAsync(stored.StorageKey);
            using var actual = new MemoryStream();
            await opened.CopyToAsync(actual);

            Assert.Equal(expected, actual.ToArray());
        }
        finally
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, recursive: true);
            }
        }
    }

    private static byte[] CreateValidPdf()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        page.AddText("AptLens resume", 12, new PdfPoint(40, 760), font);
        return builder.Build();
    }

    private static byte[] CreateClassicXrefPdf()
    {
        var builder = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int>();
        var content = "BT /F1 12 Tf 72 720 Td (AptLens resume) Tj ET";

        AppendObject(1, "<< /Type /Catalog /Pages 2 0 R >>");
        AppendObject(2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        AppendObject(3, "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>");
        AppendObject(4, $"<< /Length {content.Length} >>\nstream\n{content}\nendstream");
        AppendObject(5, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

        var xrefOffset = builder.Length;
        builder.Append("xref\n0 6\n0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            builder.Append(offset.ToString("D10")).Append(" 00000 n \n");
        }

        builder.Append("trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n")
            .Append(xrefOffset)
            .Append("\n%%EOF\n");

        return Encoding.ASCII.GetBytes(builder.ToString());

        void AppendObject(int number, string body)
        {
            offsets.Add(builder.Length);
            builder.Append(number)
                .Append(" 0 obj\n")
                .Append(body)
                .Append("\nendobj\n");
        }
    }

    private sealed class NonSeekableReadStream : Stream
    {
        private readonly MemoryStream _inner;

        public NonSeekableReadStream(byte[] bytes) => _inner = new MemoryStream(bytes);
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            _inner.ReadAsync(buffer, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            if (disposing) _inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
