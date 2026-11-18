using Games.Domain.LocationChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeSeed
{
    public static readonly LocationChallenge DraftFortressHunt;
    public static readonly LocationChallenge PublishedOldTownHunt;
    public static readonly LocationChallenge RetiredParkHunt;
    public static readonly LocationChallenge ArchivedQuayHunt;

    static LocationChallengeSeed()
    {
        DraftFortressHunt = new("Potraga po Petrovaradinskoj tvrđavi", new Location(45.2517, 19.8619), 120,
        [
            new HiddenPoint(new Location(45.2531, 19.8625), ImageSeed.FortressGatePhoto.Id),
            new HiddenPoint(new Location(45.2522, 19.8641), ImageSeed.ClockTowerPhoto.Id)
        ]);

        PublishedOldTownHunt = new("Potraga kroz Stari grad", new Location(45.2551, 19.8451), 80,
            [new HiddenPoint(new Location(45.2553, 19.8449), ImageSeed.CathedralPhoto.Id)]);
        PublishedOldTownHunt.Publish();

        RetiredParkHunt = new("Potraga kroz Dunavski park", new Location(45.2549, 19.8510), 60,
            [new HiddenPoint(new Location(45.2546, 19.8519), ImageSeed.ParkFountainPhoto.Id)]);
        RetiredParkHunt.Publish();
        RetiredParkHunt.Retire();

        ArchivedQuayHunt = new("Potraga duž keja", new Location(45.2466, 19.8556), 50,
            [new HiddenPoint(new Location(45.2459, 19.8570), ImageSeed.QuayBridgePhoto.Id)]);
        ArchivedQuayHunt.Publish();
        ArchivedQuayHunt.Retire();
        ArchivedQuayHunt.Archive();
    }

    public static LocationChallenge[] All => [DraftFortressHunt, PublishedOldTownHunt, RetiredParkHunt, ArchivedQuayHunt];
}
