using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Images;

public sealed class Image : AggregateRoot
{
    private static readonly TimeSpan UnattachedLifetime = TimeSpan.FromHours(24);

    public Guid UploaderId { get; private set; }
    public string StoredFileName { get; private set; }
    public ImageDimensions Dimensions { get; private set; }
    public long SizeBytes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ImageStatus Status { get; private set; }
    public DateTime? AttachedAt { get; private set; }

    private Image()
    {
        StoredFileName = null!;
        Dimensions = null!;
    }

    public Image(Guid uploaderId, ImageDimensions dimensions, long sizeBytes)
        : this(uploaderId, dimensions, sizeBytes, DateTime.UtcNow)
    {
    }

    public Image(Guid uploaderId, ImageDimensions dimensions, long sizeBytes, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        UploaderId = uploaderId;
        StoredFileName = $"{Id:N}.webp";
        Dimensions = dimensions;
        SizeBytes = sizeBytes;
        CreatedAt = createdAt;
        Status = ImageStatus.Unattached;
    }

    public void AttachTo(Guid userId)
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
