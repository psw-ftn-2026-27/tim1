using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class Club : AggregateRoot
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<string> Images { get; private set; }

    private Club()
    {
        Name = null!;
        Description = null!;
        Images = null!;
    }

    public Club(Guid ownerId, string name, string description, List<string>? images)
        : base(Guid.NewGuid())
    {
        OwnerId = ownerId;
        Name = null!;
        Description = null!;
        Images = null!;
        SetDetails(name, description, images);
    }

    public void Update(string name, string description, List<string>? images)
    {
        SetDetails(name, description, images);
    }

    private void SetDetails(string name, string description, List<string>? images)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A club requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A club requires a description.");
        }
        if (images is null || images.Count == 0)
        {
            throw new DomainException("A club requires at least one image.");
        }

        Name = name.Trim();
        Description = description;
        Images = images;
    }
}
