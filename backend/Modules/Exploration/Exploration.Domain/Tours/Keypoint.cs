using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class Keypoint : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid ImageId { get; private set; }
    public string? Secret { get; private set; }
    public Coordinate Location { get; private set; }
    public bool PubliclyVisible { get; private set; }

    private Keypoint()
    {
        Name = null!;
        Description = null!;
        Location = null!;
    }

    internal Keypoint(string name, string description, Guid imageId, string? secret, bool publiclyVisible, Coordinate location)
        : base(Guid.NewGuid())
    {
        Validate(name, description, imageId, location);

        Name = name;
        Description = description;
        ImageId = imageId;
        Secret = secret;
        Location = location;
        PubliclyVisible = publiclyVisible;
    }

    internal void Update(string name, string description, Guid imageId, string? secret, Coordinate location)
    {
        Validate(name, description, imageId, location);

        Name = name;
        Description = description;
        ImageId = imageId;
        Secret = secret;
        Location = location;
    }

    internal void SetPubliclyVisible(bool publiclyVisible)
    {
        PubliclyVisible = publiclyVisible;
    }

    private static void Validate(string name, string description, Guid imageId, Coordinate location)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A keypoint requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A keypoint requires a description.");
        }
        if (imageId == Guid.Empty)
        {
            throw new DomainException("A keypoint requires an image.");
        }
        if (location is null)
        {
            throw new DomainException("A keypoint requires a location.");
        }
    }
}
