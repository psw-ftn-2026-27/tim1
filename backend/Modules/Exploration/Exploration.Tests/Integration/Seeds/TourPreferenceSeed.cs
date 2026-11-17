using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourPreferenceSeed
{
    public static readonly TourPreference ExplorersPreference = new(WellKnownUsers.Explorer, [TourDifficulty.Easy, TourDifficulty.Moderate], TransportMode.Walking, ["istorija"], true);
    public static readonly TourPreference SecondExplorersPreference = new(WellKnownUsers.SecondExplorer, [TourDifficulty.Hard], null, ["vino"], false);

    public static TourPreference[] All => [ExplorersPreference, SecondExplorersPreference];
}
