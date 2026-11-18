using Games.Domain.LocationChallengeAttempts;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeAttemptSeed
{
    public static readonly LocationChallengeAttempt ExplorersOldTownAttempt;
    public static readonly LocationChallengeAttempt ExplorersSquareAttemptWithOnePointDiscovered;
    public static readonly LocationChallengeAttempt SecondExplorersSolvedSquareAttempt;

    static LocationChallengeAttemptSeed()
    {
        var oldTownHunt = LocationChallengeSeed.PublishedOldTownHunt;
        ExplorersOldTownAttempt = new(
            oldTownHunt.Id,
            WellKnownUsers.Explorer,
            new SearchCircle(oldTownHunt.StartLocation, [oldTownHunt.HiddenPoints[0].Location], 40));

        var squareHunt = LocationChallengeSeed.PublishedSquareHunt;
        var squareCircle = new SearchCircle(
            squareHunt.StartLocation,
            [squareHunt.HiddenPoints[0].Location, squareHunt.HiddenPoints[1].Location],
            25);

        ExplorersSquareAttemptWithOnePointDiscovered = new(squareHunt.Id, WellKnownUsers.Explorer, squareCircle);
        ExplorersSquareAttemptWithOnePointDiscovered.Discover([squareHunt.HiddenPoints[0].Id], squareHunt.HiddenPoints.Count);

        SecondExplorersSolvedSquareAttempt = new(squareHunt.Id, WellKnownUsers.SecondExplorer, squareCircle);
        SecondExplorersSolvedSquareAttempt.Discover([squareHunt.HiddenPoints[1].Id], squareHunt.HiddenPoints.Count);
        SecondExplorersSolvedSquareAttempt.Discover([squareHunt.HiddenPoints[0].Id], squareHunt.HiddenPoints.Count);
    }

    public static LocationChallengeAttempt[] All =>
        [ExplorersOldTownAttempt, ExplorersSquareAttemptWithOnePointDiscovered, SecondExplorersSolvedSquareAttempt];
}
