using Shared.Domain;

namespace Games.Domain.Images;

/// <summary>
/// The record of an uploaded challenge photo. The file itself lives in image storage under
/// StoredFileName, which the system chooses. Width and Height describe the stored image and
/// are plain data: the shared image processor already checked the limits.
/// </summary>
public sealed class Image : AggregateRoot
{
    public string StoredFileName { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public long SizeBytes { get; private set; }

    private Image()
    {
        StoredFileName = null!;
    }

    public Image(int width, int height, long sizeBytes)
        : base(Guid.NewGuid())
    {
        StoredFileName = $"{Id:N}.webp";
        Width = width;
        Height = height;
        SizeBytes = sizeBytes;
    }
}
