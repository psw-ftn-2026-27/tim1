using Games.Domain.ChallengeCompletions;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeAttempts;

/// <summary>
/// Checks whether an explorer stands on a hidden point of the challenge they are solving.
/// The rule needs the hidden points' locations from the challenge and the discovered points
/// from the attempt, so it lives here rather than in either aggregate. Discovering the last
/// hidden point hands back the completion that awards the experience points; any other
/// discovery returns nothing.
/// </summary>
public sealed class LocationDiscovery
{
    public const double MaxDistanceMeters = 5;

    // One check discovers every undiscovered hidden point within reach. A failed check does
    // not say how far the hidden points are.
    public ChallengeCompletion? Check(LocationChallenge challenge, LocationChallengeAttempt attempt, Location position)
    {
        if (attempt.ChallengeId != challenge.Id)
        {
            throw new DomainException("An attempt can discover only the hidden points of its own challenge.");
        }
        if (attempt.IsSolved)
        {
            throw new DomainException("This location challenge is already solved.");
        }

        var discovered = challenge.HiddenPoints
            .Where(point => !attempt.DiscoveredPointIds.Contains(point.Id))
            .Where(point => position.DistanceTo(point.Location) <= MaxDistanceMeters)
            .Select(point => point.Id)
            .ToList();
        if (discovered.Count == 0)
        {
            throw new DomainException("You are not at a hidden location.");
        }

        attempt.Discover(discovered, challenge.HiddenPoints.Count);
        if (!attempt.IsSolved)
        {
            return null;
        }

        return new ChallengeCompletion(
            attempt.UserId, challenge.Id, ChallengeType.Location, attempt.SolvedAt!.Value, challenge.ExperiencePoints);
    }
}
