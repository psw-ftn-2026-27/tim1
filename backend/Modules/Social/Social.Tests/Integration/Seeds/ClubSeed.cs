using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

internal static class ClubSeed
{
    public static readonly Club HikersClub = new(WellKnownUsers.Explorer, "Planinari", "Klub ljubitelja planinarenja po Fruškoj gori.", ["https://example.com/planinari.jpg"]);
    public static readonly Club PhotographersClub = new(WellKnownUsers.Explorer, "Fotografi", "Klub ljubitelja fotografisanja gradskih znamenitosti.", ["https://example.com/fotografi.jpg", "https://example.com/fotografi-2.jpg"]);
    public static readonly Club SecondExplorersCyclingClub = new(WellKnownUsers.SecondExplorer, "Biciklisti", "Klub drugog osnivača za vožnju bicikla uz Dunav.", ["https://example.com/biciklisti.jpg"]);

    public static Club[] All => [HikersClub, PhotographersClub, SecondExplorersCyclingClub];
}
