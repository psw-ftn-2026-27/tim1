using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge DraftQuayWalk = new("Večernja šetnja kejom", new TimeOnly(19, 0), new TimeOnly(21, 0), 5, 150, 50, "Bravo, zajedno ste prošetali kej!", 45.2466, 19.8556);
    public static readonly SocialChallenge PublishedFortressYoga;
    public static readonly SocialChallenge RetiredParkRun;
    public static readonly SocialChallenge ArchivedBeachPicnic;

    static SocialChallengeSeed()
    {
        PublishedFortressYoga = new("Joga na tvrđavi", new TimeOnly(6, 30), new TimeOnly(8, 0), 4, 100, 80, "Izazov je završen, hvala na zajedničkom vežbanju!", 45.2531, 19.8625);
        PublishedFortressYoga.Publish();

        RetiredParkRun = new("Trčanje kroz Dunavski park", new TimeOnly(12, 0), new TimeOnly(13, 30), 3, 200, 60, "Odlično trčanje, vidimo se ponovo!", 45.2549, 19.8510);
        RetiredParkRun.Publish();
        RetiredParkRun.Retire();

        ArchivedBeachPicnic = new("Piknik na Štrandu", new TimeOnly(15, 0), new TimeOnly(18, 0), 6, 250, 100, "Hvala na druženju na Štrandu!", 45.2436, 19.8546);
        ArchivedBeachPicnic.Publish();
        ArchivedBeachPicnic.Retire();
        ArchivedBeachPicnic.Archive();
    }

    public static SocialChallenge[] All => [DraftQuayWalk, PublishedFortressYoga, RetiredParkRun, ArchivedBeachPicnic];
}
