using Shared.Domain.Exceptions;
using SkiaSharp;

namespace Shared.Infrastructure.Images;

/// <summary>
/// Shared local technical expert for uploaded images; README.md next to this file lists
/// every check and why it exists. It holds the security limits that are the same for
/// every module (P2-P8) and takes the module's purpose limits (P10, P11) as input.
/// A rejected file throws DomainException, which the host turns into 400. Purpose limits
/// outside the platform frame are a bug in the calling module, not a user error, and
/// throw ArgumentOutOfRangeException.
/// </summary>
public sealed class ImageProcessor
{
    /// <summary>P2. The single source of the 5 MB number, also used by the host's request body limit.</summary>
    public const int MaxFileBytes = 5 * 1024 * 1024;

    /// <summary>P4. Upper bounds checked from the header, before full decoding.</summary>
    public const int MaxSide = 8000;
    public const long MaxPixels = 40_000_000;

    /// <summary>P10. The smallest minimum side a module may choose, and the default.</summary>
    public const int MinSideFloor = 100;

    /// <summary>P11. The largest long side a module may choose, and the default.</summary>
    public const int MaxLongSideCeiling = 2048;

    /// <summary>P6. Every stored image is re-encoded as WebP.</summary>
    public const string ContentType = "image/webp";

    private const int WebpQuality = 85;

    public ProcessedImage Process(Stream content, int minSide = MinSideFloor, int maxLongSide = MaxLongSideCeiling)
    {
        EnsureWithinPlatformFrame(minSide, nameof(minSide));
        EnsureWithinPlatformFrame(maxLongSide, nameof(maxLongSide));

        var bytes = ReadAtMostMaxFileBytes(content);
        if (!HasSupportedSignature(bytes))
        {
            throw new DomainException("Only JPEG, PNG and WebP images are accepted.");
        }

        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data) ?? throw new DomainException("The file is not a valid image.");
        EnsureDimensions(codec.Info.Width, codec.Info.Height, minSide);

        using var decoded = Decode(codec);
        using var oriented = ApplyOrigin(decoded, codec.EncodedOrigin);
        using var shrunk = ShrinkLongSide(oriented, maxLongSide);
        var stored = shrunk ?? oriented;
        using var image = SKImage.FromBitmap(stored);
        using var encoded = image.Encode(SKEncodedImageFormat.Webp, WebpQuality);

        return new ProcessedImage(encoded.ToArray(), stored.Width, stored.Height);
    }

    private static void EnsureWithinPlatformFrame(int purposeLimit, string name)
    {
        if (purposeLimit < MinSideFloor || purposeLimit > MaxLongSideCeiling)
        {
            throw new ArgumentOutOfRangeException(name, purposeLimit,
                $"A purpose limit must be between {MinSideFloor} and {MaxLongSideCeiling} pixels.");
        }
    }

    private static byte[] ReadAtMostMaxFileBytes(Stream content)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = content.Read(chunk, 0, chunk.Length)) > 0)
        {
            buffer.Write(chunk, 0, read);
            if (buffer.Length > MaxFileBytes)
            {
                throw new DomainException("The image is larger than 5 MB.");
            }
        }
        return buffer.ToArray();
    }

    private static bool HasSupportedSignature(byte[] bytes)
    {
        var isJpeg = bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        var isPng = bytes.Length >= 8
            && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var isWebp = bytes.Length >= 12
            && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        return isJpeg || isPng || isWebp;
    }

    // The codec has read only the header so far, so these checks run before any pixel is decoded.
    private static void EnsureDimensions(int width, int height, int minSide)
    {
        if (width > MaxSide || height > MaxSide || (long)width * height > MaxPixels)
        {
            throw new DomainException(
                $"An image can be at most {MaxSide}x{MaxSide} pixels and {MaxPixels / 1_000_000} million pixels in total.");
        }
        if (width < minSide || height < minSide)
        {
            throw new DomainException($"An image must be at least {minSide}x{minSide} pixels.");
        }
    }

    private static SKBitmap Decode(SKCodec codec)
    {
        var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var bitmap = new SKBitmap(info);
        if (codec.GetPixels(info, bitmap.GetPixels()) != SKCodecResult.Success)
        {
            bitmap.Dispose();
            throw new DomainException("The file is not a valid image.");
        }
        return bitmap;
    }

    // Draws the pixels the way the EXIF orientation says they should be shown, because the
    // re-encoded file carries no EXIF and so no orientation of its own (P7).
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

    // Returns null when the image already fits: a smaller image is never enlarged (P11).
    private static SKBitmap? ShrinkLongSide(SKBitmap source, int maxLongSide)
    {
        var longSide = Math.Max(source.Width, source.Height);
        if (longSide <= maxLongSide)
        {
            return null;
        }

        var scale = (double)maxLongSide / longSide;
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));
        return source.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
    }
}
