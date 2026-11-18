using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class HiddenPoint : Entity
{
    public Location Location { get; private set; }
    public Guid ImageId { get; private set; }

    private HiddenPoint()
    {
        Location = null!;
    }

    public HiddenPoint(Location location, Guid imageId)
        : base(Guid.NewGuid())
    {
        if (imageId == Guid.Empty)
        {
            throw new DomainException("A hidden point requires a photo.");
        }

        Location = location;
        ImageId = imageId;
    }
}
