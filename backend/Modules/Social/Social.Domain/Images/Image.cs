using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Images;

/// <summary>
/// The record of an uploaded image. The file itself lives in image storage under
/// StoredFileName, which the system chooses (P8). Width and Height describe the stored
/// image and are plain data: the shared image processor already checked the limits.
/// </summary>
public sealed class Image : AggregateRoot
{
    private static readonly TimeSpan UnattachedLifetime = TimeSpan.FromHours(24);

    public Guid UploaderId { get; private set; }
    public string StoredFileName { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public long SizeBytes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ImageStatus Status { get; private set; }
    public DateTime? AttachedAt { get; private set; }

    private Image()
    {
        StoredFileName = null!;
    }

    public Image(Guid uploaderId, int width, int height, long sizeBytes)
        : this(uploaderId, width, height, sizeBytes, DateTime.UtcNow)
    {
    }

    /// <summary>Takes the upload time explicitly, so test seeds can describe an image uploaded in the past.</summary>
    public Image(Guid uploaderId, int width, int height, long sizeBytes, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        UploaderId = uploaderId;
        StoredFileName = $"{Id:N}.webp";
        Width = width;
        Height = height;
        SizeBytes = sizeBytes;
        CreatedAt = createdAt;
        Status = ImageStatus.Unattached;
    }

    public void AttachToBlog(Guid userId)
    {
        if (userId != UploaderId)
        {
            throw new DomainException("An image can be attached only by the user who uploaded it.");
        }
        if (Status != ImageStatus.Unattached)
        {
            throw new DomainException("The image is already attached to a blog.");
        }

        Status = ImageStatus.Attached;
        AttachedAt = DateTime.UtcNow;
    }

    public bool IsExpired(DateTime now)
    {
        return Status == ImageStatus.Unattached && now - CreatedAt > UnattachedLifetime;
    }
}
