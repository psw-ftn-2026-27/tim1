using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeAttempts;

/// <summary>
/// The area in which an explorer searches for the hidden points of a location challenge.
/// The diameter is the distance from the center to the farthest hidden point plus a random
/// extra, so some hidden points can lie outside the circle (an accepted trade-off of the rule).
/// </summary>
public sealed record SearchCircle
{
    public const double MaxExtraMeters = 100;

    public Location Center { get; private init; }
    public double DiameterMeters { get; private init; }

    private SearchCircle()
    {
        Center = null!;
    }

    public SearchCircle(Location center, IReadOnlyList<Location> hiddenPointLocations, double extraMeters)
    {
        if (hiddenPointLocations.Count == 0)
        {
            throw new DomainException("A search circle requires at least one hidden point.");
        }
        if (extraMeters is not (>= 0 and <= MaxExtraMeters))
        {
            throw new DomainException($"A search circle requires an extra between 0 and {MaxExtraMeters} m.");
        }

        Center = center;
        DiameterMeters = hiddenPointLocations.Max(center.DistanceTo) + extraMeters;
    }
}
