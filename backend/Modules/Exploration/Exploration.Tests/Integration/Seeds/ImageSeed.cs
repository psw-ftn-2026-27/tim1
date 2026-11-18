using Exploration.Domain.Images;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class ImageSeed
{
    public static readonly Image DraftFirstKeypointPhoto;
    public static readonly Image DraftSecondKeypointPhoto;
    public static readonly Image RiversideStartPhoto;
    public static readonly Image RiversideEndPhoto;
    public static readonly Image VineyardsStartPhoto;
    public static readonly Image VineyardsEndPhoto;
    public static readonly Image MonasteriesStartPhoto;
    public static readonly Image MonasteriesEndPhoto;
    public static readonly Image LakesideStartPhoto;
    public static readonly Image LakesideEndPhoto;
    public static readonly Image OrchardsStartPhoto;
    public static readonly Image OrchardsEndPhoto;
    public static readonly Image ExplorersUnattachedImage = new(WellKnownUsers.Explorer, 800, 600, 1000);
    public static readonly Image SecondExplorersUnattachedImage = new(WellKnownUsers.SecondExplorer, 800, 600, 1000);
    public static readonly Image ExpiredUnattachedImage = new(WellKnownUsers.Explorer, 800, 600, 1000, DateTime.UtcNow.AddHours(-30));

    static ImageSeed()
    {
        DraftFirstKeypointPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        DraftFirstKeypointPhoto.AttachToKeypoint(WellKnownUsers.Explorer);

        DraftSecondKeypointPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        DraftSecondKeypointPhoto.AttachToKeypoint(WellKnownUsers.Explorer);

        RiversideStartPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        RiversideStartPhoto.AttachToKeypoint(WellKnownUsers.Explorer);

        RiversideEndPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        RiversideEndPhoto.AttachToKeypoint(WellKnownUsers.Explorer);

        VineyardsStartPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        VineyardsStartPhoto.AttachToKeypoint(WellKnownUsers.Explorer);

        VineyardsEndPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        VineyardsEndPhoto.AttachToKeypoint(WellKnownUsers.Explorer);

        MonasteriesStartPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        MonasteriesStartPhoto.AttachToKeypoint(WellKnownUsers.Explorer);

        MonasteriesEndPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        MonasteriesEndPhoto.AttachToKeypoint(WellKnownUsers.Explorer);

        LakesideStartPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        LakesideStartPhoto.AttachToKeypoint(WellKnownUsers.Explorer);

        LakesideEndPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        LakesideEndPhoto.AttachToKeypoint(WellKnownUsers.Explorer);

        OrchardsStartPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        OrchardsStartPhoto.AttachToKeypoint(WellKnownUsers.Explorer);

        OrchardsEndPhoto = new(WellKnownUsers.Explorer, 800, 600, 1000);
        OrchardsEndPhoto.AttachToKeypoint(WellKnownUsers.Explorer);
    }

    public static Image[] All =>
    [
        DraftFirstKeypointPhoto,
        DraftSecondKeypointPhoto,
        RiversideStartPhoto,
        RiversideEndPhoto,
        VineyardsStartPhoto,
        VineyardsEndPhoto,
        MonasteriesStartPhoto,
        MonasteriesEndPhoto,
        LakesideStartPhoto,
        LakesideEndPhoto,
        OrchardsStartPhoto,
        OrchardsEndPhoto,
        ExplorersUnattachedImage,
        SecondExplorersUnattachedImage,
        ExpiredUnattachedImage
    ];
}
