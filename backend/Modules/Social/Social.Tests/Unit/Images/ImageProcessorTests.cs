using System.Text;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Infrastructure.Images;
using SkiaSharp;
using Xunit;

namespace Social.Tests.Unit.Images;

// The shared image processor lives in Shared.Infrastructure; Social is its first user, so
// its unit tests live here rather than in a new test project.
public class ImageProcessorTests
{
    private readonly ImageProcessor _processor = new();

    private static SKImageInfo ReadInfo(byte[] content)
    {
        using var data = SKData.CreateCopy(content);
        using var codec = SKCodec.Create(data);
        return codec!.Info;
    }

    private static byte[] PngPaddedTo(int totalBytes)
    {
        var png = TestImages.Png(300, 200);
        return png.Concat(new byte[totalBytes - png.Length]).ToArray();
    }

    [Fact]
    public void File_of_exactly_5_megabytes_is_accepted()
    {
        using var content = new MemoryStream(PngPaddedTo(ImageProcessor.MaxFileBytes));

        var processing = () => _processor.Process(content);

        processing.Should().NotThrow();
    }

    [Fact]
    public void File_larger_than_5_megabytes_is_rejected()
    {
        using var content = new MemoryStream(PngPaddedTo(ImageProcessor.MaxFileBytes + 1));

        var processing = () => _processor.Process(content);

        processing.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("this is plain text named photo.jpg")]
    [InlineData("""<svg xmlns="http://www.w3.org/2000/svg" width="200" height="200"><rect width="200" height="200"/></svg>""")]
    public void File_pretending_to_be_an_image_is_rejected(string text)
    {
        using var content = new MemoryStream(Encoding.UTF8.GetBytes(text));

        var processing = () => _processor.Process(content);

        processing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Image_in_a_format_other_than_jpeg_png_or_webp_is_rejected()
    {
        using var content = new MemoryStream(TestImages.Bmp(300, 200));

        var processing = () => _processor.Process(content);

        processing.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(99, 600, 100)]
    [InlineData(600, 99, 100)]
    [InlineData(8001, 100, 100)]
    [InlineData(100, 8001, 100)]
    [InlineData(6400, 6400, 100)]
    [InlineData(250, 400, 300)]
    public void Image_outside_the_dimension_frame_is_rejected(int width, int height, int minSide)
    {
        using var content = new MemoryStream(TestImages.Png(width, height));

        var processing = () => _processor.Process(content, minSide);

        processing.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(100, 100, 100)]
    [InlineData(8000, 100, 100)]
    [InlineData(300, 400, 300)]
    public void Image_inside_the_dimension_frame_is_accepted(int width, int height, int minSide)
    {
        using var content = new MemoryStream(TestImages.Png(width, height));

        var processing = () => _processor.Process(content, minSide);

        processing.Should().NotThrow();
    }

    [Fact]
    public void File_that_cannot_be_decoded_is_rejected()
    {
        var png = TestImages.Png(300, 200);
        using var content = new MemoryStream(png.Take(png.Length / 2).ToArray());

        var processing = () => _processor.Process(content);

        processing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Stored_image_is_re_encoded_without_metadata()
    {
        var original = TestImages.JpegWithExif(300, 200, 1, "GPS-SECRET-MARKER");
        using var content = new MemoryStream(original);

        var processed = _processor.Process(content);

        Encoding.ASCII.GetString(original).Should().Contain("GPS-SECRET-MARKER");
        Encoding.ASCII.GetString(processed.Content).Should().NotContain("GPS-SECRET-MARKER").And.NotContain("Exif");
        ReadEncodedFormat(processed.Content).Should().Be(SKEncodedImageFormat.Webp);
    }

    [Fact]
    public void Stored_image_keeps_the_orientation_of_the_original()
    {
        using var content = new MemoryStream(TestImages.JpegWithExif(300, 200, 6, "marker"));

        var processed = _processor.Process(content);

        var stored = ReadInfo(processed.Content);
        stored.Width.Should().Be(200);
        stored.Height.Should().Be(300);
        processed.Width.Should().Be(200);
        processed.Height.Should().Be(300);
    }

    [Theory]
    [InlineData(ImageProcessor.MaxLongSideCeiling, 2048, 1024)]
    [InlineData(1000, 1000, 500)]
    public void Long_side_of_a_large_image_is_shrunk_to_the_chosen_limit(int maxLongSide, int expectedWidth, int expectedHeight)
    {
        using var content = new MemoryStream(TestImages.Png(3000, 1500));

        var processed = _processor.Process(content, maxLongSide: maxLongSide);

        var stored = ReadInfo(processed.Content);
        stored.Width.Should().Be(expectedWidth);
        stored.Height.Should().Be(expectedHeight);
        processed.Width.Should().Be(expectedWidth);
        processed.Height.Should().Be(expectedHeight);
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

    [Theory]
    [InlineData(99, 2048)]
    [InlineData(2049, 2048)]
    [InlineData(100, 99)]
    [InlineData(100, 2049)]
    public void Purpose_limit_outside_the_platform_frame_is_a_programming_error(int minSide, int maxLongSide)
    {
        using var content = new MemoryStream(TestImages.Png(300, 200));

        var processing = () => _processor.Process(content, minSide, maxLongSide);

        processing.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static SKEncodedImageFormat ReadEncodedFormat(byte[] content)
    {
        using var data = SKData.CreateCopy(content);
        using var codec = SKCodec.Create(data);
        return codec!.EncodedFormat;
    }
}
