using SkiaSharp;

namespace Social.Tests;

internal static class TestImages
{
    public static byte[] Png(int width, int height) => Encode(width, height, SKEncodedImageFormat.Png);

    public static byte[] Jpeg(int width, int height) => Encode(width, height, SKEncodedImageFormat.Jpeg);

    /// <summary>
    /// A JPEG that carries an EXIF segment with the given orientation and a marker text,
    /// inserted right after the start-of-image bytes.
    /// </summary>
    public static byte[] JpegWithExif(int width, int height, ushort orientation, string marker)
    {
        var jpeg = Jpeg(width, height);

        var markerBytes = System.Text.Encoding.ASCII.GetBytes(marker + "\0");
        var tiff = new List<byte> { (byte)'M', (byte)'M', 0, 42, 0, 0, 0, 8 };
        tiff.AddRange([0, 1]);
        tiff.AddRange([0x01, 0x12, 0, 3, 0, 0, 0, 1, (byte)(orientation >> 8), (byte)orientation, 0, 0]);
        tiff.AddRange([0, 0, 0, 0]);
        tiff.AddRange(markerBytes);

        var payload = new List<byte>("Exif\0\0"u8.ToArray());
        payload.AddRange(tiff);
        var length = payload.Count + 2;

        var result = new List<byte> { 0xFF, 0xD8, 0xFF, 0xE1, (byte)(length >> 8), (byte)length };
        result.AddRange(payload);
        result.AddRange(jpeg.Skip(2));
        return result.ToArray();
    }

    private static byte[] Encode(int width, int height, SKEncodedImageFormat format)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.SteelBlue);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 90);
        return data.ToArray();
    }
}
