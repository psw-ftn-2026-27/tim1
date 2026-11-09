using Shared.Domain.Exceptions;
using SkiaSharp;
using Social.Application.Images;
using Social.Domain.Images;

namespace Social.Infrastructure.Images;

/// <summary>
/// Local technical expert for uploaded images. Accepts only JPEG, PNG and WebP (recognised
/// by content), checks the size from the header before decoding, applies the EXIF
/// orientation, shrinks the long side to 2048 px and re-encodes as WebP, which drops all
/// metadata. The original bytes are never kept.
/// </summary>
internal sealed class ImageProcessor : IImageProcessor
{
    public const int MaxFileBytes = 5 * 1024 * 1024;
    public const int MaxSide = 2048;
    private const int WebpQuality = 85;

    public ProcessedImage Process(Stream content)
    {
        var bytes = ReadAll(content);
        if (!HasSupportedSignature(bytes))
        {
            throw new DomainException("Only JPEG, PNG and WebP images are accepted.");
        }

        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data);
        if (codec is null)
        {
            throw new DomainException("The file is not a valid image.");
        }

        var original = new ImageDimensions(codec.Info.Width, codec.Info.Height);

        using var decoded = SKBitmap.Decode(codec, new SKImageInfo(original.Width, original.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        if (decoded is null)
        {
            throw new DomainException("The file is not a valid image.");
        }

        using var oriented = ApplyOrigin(decoded, codec.EncodedOrigin);
        using var resized = ShrinkToMaxSide(oriented);
        using var image = SKImage.FromBitmap(resized ?? oriented);
        using var encoded = image.Encode(SKEncodedImageFormat.Webp, WebpQuality);

        return new ProcessedImage(encoded.ToArray(), original);
    }

    private static byte[] ReadAll(Stream content)
    {
        using var buffer = new MemoryStream();
        var limited = new byte[81920];
        int read;
        while ((read = content.Read(limited, 0, limited.Length)) > 0)
        {
            buffer.Write(limited, 0, read);
            if (buffer.Length > MaxFileBytes)
            {
                throw new DomainException("The image is larger than 5 MB.");
            }
        }
        return buffer.ToArray();
    }

    private static bool HasSupportedSignature(byte[] bytes)
    {
        var isJpeg = bytes.Length > 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        var isPng = bytes.Length > 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var isWebp = bytes.Length > 12
            && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        return isJpeg || isPng || isWebp;
    }

    private static SKBitmap ApplyOrigin(SKBitmap source, SKEncodedOrigin origin)
    {
        float width = source.Width;
        float height = source.Height;
        var swapsSides = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

        var matrix = origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, width, 0, 1, 0, 0, 0, 1),
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, width, 0, -1, height, 0, 0, 1),
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, height, 0, 0, 1),
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, height, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, height, -1, 0, width, 0, 0, 1),
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, width, 0, 0, 1),
            _ => SKMatrix.Identity
        };

        var result = new SKBitmap(
            swapsSides ? source.Height : source.Width,
            swapsSides ? source.Width : source.Height);
        using var canvas = new SKCanvas(result);
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(source, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
        return result;
    }

    private static SKBitmap? ShrinkToMaxSide(SKBitmap source)
    {
        var longSide = Math.Max(source.Width, source.Height);
        if (longSide <= MaxSide)
        {
            return null;
        }

        var scale = (double)MaxSide / longSide;
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));
        return source.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
    }
}
