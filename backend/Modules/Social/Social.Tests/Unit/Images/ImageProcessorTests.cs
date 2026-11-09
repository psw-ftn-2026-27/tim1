using System.Text;
using FluentAssertions;
using Shared.Domain.Exceptions;
using SkiaSharp;
using Social.Infrastructure.Images;
using Xunit;

namespace Social.Tests.Unit.Images;

public class ImageProcessorTests
{
    private readonly ImageProcessor _processor = new();

    private static SKImageInfo ReadInfo(byte[] content)
    {
        using var data = SKData.CreateCopy(content);
        using var codec = SKCodec.Create(data);
        return codec!.Info;
    }

    [Fact]
    public void File_pretending_to_be_an_image_is_rejected()
    {
        using var content = new MemoryStream(Encoding.UTF8.GetBytes("this is plain text named photo.jpg"));

        var processing = () => _processor.Process(content);

        processing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Svg_is_rejected()
    {
        var svg = """<svg xmlns="http://www.w3.org/2000/svg" width="200" height="200"><rect width="200" height="200"/></svg>""";
        using var content = new MemoryStream(Encoding.UTF8.GetBytes(svg));

        var processing = () => _processor.Process(content);

        processing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Stored_image_has_no_metadata()
    {
        var original = TestImages.JpegWithExif(300, 200, 1, "GPS-SECRET-MARKER");
        using var content = new MemoryStream(original);

        var processed = _processor.Process(content);

        Encoding.ASCII.GetString(original).Should().Contain("GPS-SECRET-MARKER");
        Encoding.ASCII.GetString(processed.Content).Should().NotContain("GPS-SECRET-MARKER").And.NotContain("Exif");
        processed.Content.Should().NotEqual(original);
    }

    [Fact]
    public void Stored_image_keeps_the_orientation_of_the_original()
    {
        using var content = new MemoryStream(TestImages.JpegWithExif(300, 200, 6, "marker"));

        var processed = _processor.Process(content);

        var stored = ReadInfo(processed.Content);
        stored.Width.Should().Be(200);
        stored.Height.Should().Be(300);
    }

    [Fact]
    public void Long_side_of_a_large_image_is_shrunk_to_2048_pixels()
    {
        using var content = new MemoryStream(TestImages.Png(3000, 1500));

        var processed = _processor.Process(content);

        var stored = ReadInfo(processed.Content);
        stored.Width.Should().Be(2048);
        stored.Height.Should().Be(1024);
    }

    [Fact]
    public void Small_image_is_not_enlarged()
    {
        using var content = new MemoryStream(TestImages.Png(300, 200));

        var processed = _processor.Process(content);

        var stored = ReadInfo(processed.Content);
        stored.Width.Should().Be(300);
        stored.Height.Should().Be(200);
    }

    [Fact]
    public void Image_below_the_minimum_size_is_rejected_before_decoding()
    {
        using var content = new MemoryStream(TestImages.Png(50, 50));

        var processing = () => _processor.Process(content);

        processing.Should().Throw<DomainException>();
    }
}
