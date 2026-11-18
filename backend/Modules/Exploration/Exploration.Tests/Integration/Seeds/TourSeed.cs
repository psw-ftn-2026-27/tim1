using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"], new GroupSize(1, 15));
    public static readonly Tour CityStroll = new(WellKnownUsers.Explorer, "Šetnja centrom", "Kratak opis gradske šetnje.", TourDifficulty.Moderate, ["grad"], new GroupSize(2, 10));
    public static readonly Tour SecondExplorersTrail = new(WellKnownUsers.SecondExplorer, "Tuđa staza", "Kratak opis staze drugog autora.", TourDifficulty.Easy, ["grad"], new GroupSize(1, 8));
    public static readonly Tour PublishableRiverside;
    public static readonly Tour PublishedVineyards;
    public static readonly Tour PublishedMonasteries;
    public static readonly Tour PublishedLakeside;
    public static readonly Tour ArchivedOrchards;
    public static readonly Tour DraftWithKeypoints;

    static TourSeed()
    {
        DraftWithKeypoints = new(WellKnownUsers.Explorer, "Nacrt sa tačkama", "Kratak opis nacrta ture sa ključnim tačkama.", TourDifficulty.Easy, ["grad"], new GroupSize(1, 10));
        DraftWithKeypoints.AddKeypoint("Prva tačka", "Opis prve tačke.", ImageSeed.DraftFirstKeypointPhoto.Id, null, false, new Coordinate(45.2517, 19.8369));
        DraftWithKeypoints.AddKeypoint("Druga tačka", "Opis druge tačke.", ImageSeed.DraftSecondKeypointPhoto.Id, "Tajna druge tačke", true, new Coordinate(45.2671, 19.8335));

        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"], new GroupSize(1, 12));
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);
        PublishableRiverside.AddKeypoint("Početak staze", "Polazna tačka ture.", ImageSeed.RiversideStartPhoto.Id, null, true, new Coordinate(45.2517, 19.8369));
        PublishableRiverside.AddKeypoint("Kraj staze", "Završna tačka ture.", ImageSeed.RiversideEndPhoto.Id, null, false, new Coordinate(45.2671, 19.8335));

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"], new GroupSize(2, 20));
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.AddKeypoint("Početak staze", "Polazna tačka ture.", ImageSeed.VineyardsStartPhoto.Id, null, true, new Coordinate(45.2517, 19.8369));
        PublishedVineyards.AddKeypoint("Kraj staze", "Završna tačka ture.", ImageSeed.VineyardsEndPhoto.Id, null, false, new Coordinate(45.2671, 19.8335));
        PublishedVineyards.Publish(0);

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"], new GroupSize(4, 16));
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.AddKeypoint("Početak staze", "Polazna tačka ture.", ImageSeed.MonasteriesStartPhoto.Id, null, true, new Coordinate(45.2517, 19.8369));
        PublishedMonasteries.AddKeypoint("Kraj staze", "Završna tačka ture.", ImageSeed.MonasteriesEndPhoto.Id, null, false, new Coordinate(45.2671, 19.8335));
        PublishedMonasteries.Publish(0);

        PublishedLakeside = new(WellKnownUsers.Explorer, "Šetnja oko Ludaškog jezera", "Laka šetnja po stazi oko Ludaškog jezera, sa tornjevima za posmatranje ptica, drvenim mostićima i klupama za odmor tokom cele staze.", TourDifficulty.Easy, ["priroda"], new GroupSize(4, 16));
        PublishedLakeside.AddTransportTime(TransportMode.Walking, 120);
        PublishedLakeside.AddKeypoint("Početak staze", "Polazna tačka ture.", ImageSeed.LakesideStartPhoto.Id, null, true, new Coordinate(45.2517, 19.8369));
        PublishedLakeside.AddKeypoint("Kraj staze", "Završna tačka ture.", ImageSeed.LakesideEndPhoto.Id, null, false, new Coordinate(45.2671, 19.8335));
        PublishedLakeside.Publish(0);

        ArchivedOrchards = new(WellKnownUsers.Explorer, "Voćnjaci Fruške gore", "Tura vodi kroz voćnjake na južnim padinama Fruške gore, uz zaustavljanje kod seoskih domaćinstava i degustaciju domaćih sokova i rakija.", TourDifficulty.Moderate, ["priroda"], new GroupSize(4, 16));
        ArchivedOrchards.AddTransportTime(TransportMode.Bicycle, 75);
        ArchivedOrchards.AddKeypoint("Početak staze", "Polazna tačka ture.", ImageSeed.OrchardsStartPhoto.Id, null, true, new Coordinate(45.2517, 19.8369));
        ArchivedOrchards.AddKeypoint("Kraj staze", "Završna tačka ture.", ImageSeed.OrchardsEndPhoto.Id, null, false, new Coordinate(45.2671, 19.8335));
        ArchivedOrchards.Publish(0);
        ArchivedOrchards.Archive();
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublishedVineyards, PublishedMonasteries, PublishedLakeside, ArchivedOrchards, DraftWithKeypoints];
}
