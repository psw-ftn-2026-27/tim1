using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeAttempts;

/// <summary>
/// Starts an explorer's attempt at a location challenge, or continues the one already
/// started. The rules need the challenge and the attempt together, so they live here
/// rather than in either aggregate.
/// </summary>
public sealed class LocationChallengeActivation
{
    public const double MaxDistanceMeters = 50;

    public LocationChallengeAttempt Activate(
        LocationChallenge challenge,
        LocationChallengeAttempt? existingAttempt,
        Guid userId,
        Location position,
        double extraMeters)
    {
        if (existingAttempt is { IsSolved: true })
        {
            throw new DomainException("A solved location challenge cannot be activated again.");
        }
        // An attempt started before the challenge was retired can still be continued.
        if (existingAttempt is not null)
        {
            return existingAttempt;
        }

        if (challenge.Status != LocationChallengeStatus.Published)
        {
            throw new DomainException("Only a published location challenge can be activated.");
        }
        if (position.DistanceTo(challenge.StartLocation) > MaxDistanceMeters)
        {
            throw new DomainException($"Come within {MaxDistanceMeters} m of the challenge's start to activate it.");
        }

        var hiddenPointLocations = challenge.HiddenPoints.Select(point => point.Location).ToList();
        var circle = new SearchCircle(challenge.StartLocation, hiddenPointLocations, extraMeters);
        return new LocationChallengeAttempt(challenge.Id, userId, circle);
    }
}
