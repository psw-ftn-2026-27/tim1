using Games.Domain.LocationChallengeAttempts;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeAttemptSeed
{
    public static readonly LocationChallengeAttempt ExplorersOldTownAttempt = new(
        LocationChallengeSeed.PublishedOldTownHunt.Id,
        WellKnownUsers.Explorer,
        new SearchCircle(
            LocationChallengeSeed.PublishedOldTownHunt.StartLocation,
            [LocationChallengeSeed.PublishedOldTownHunt.HiddenPoints[0].Location],
            40));

    public static LocationChallengeAttempt[] All => [ExplorersOldTownAttempt];
}
