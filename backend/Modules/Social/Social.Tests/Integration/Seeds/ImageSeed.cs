using Shared.Tests;
using Social.Domain.Images;

namespace Social.Tests.Integration.Seeds;

internal static class ImageSeed
{
    public static readonly Image FortressPhoto;
    public static readonly Image ExplorersUnattachedImage = new(WellKnownUsers.Explorer, new ImageDimensions(800, 600), 1000);
    public static readonly Image SecondExplorersUnattachedImage = new(WellKnownUsers.SecondExplorer, new ImageDimensions(800, 600), 1000);
    public static readonly Image ExpiredUnattachedImage = new(WellKnownUsers.Explorer, new ImageDimensions(800, 600), 1000, DateTime.UtcNow.AddHours(-30));

    static ImageSeed()
    {
        FortressPhoto = new(WellKnownUsers.Explorer, new ImageDimensions(800, 600), 1000);
        FortressPhoto.AttachTo(WellKnownUsers.Explorer);
    }

    public static Image[] All => [FortressPhoto, ExplorersUnattachedImage, SecondExplorersUnattachedImage, ExpiredUnattachedImage];
}
