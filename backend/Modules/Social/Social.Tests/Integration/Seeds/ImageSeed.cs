using Shared.Tests;
using Social.Domain.Images;

namespace Social.Tests.Integration.Seeds;

internal static class ImageSeed
{
    public static readonly Image OldFortressPhotoOnDraft;
    public static readonly Image FortressGatePhotoOnDraft;
    public static readonly Image RiverbankPhotoOnPublishedBlog;
    public static readonly Image ExplorersUnattachedImage = new(WellKnownUsers.Explorer, 800, 600, 1000);
    public static readonly Image SecondExplorersUnattachedImage = new(WellKnownUsers.SecondExplorer, 800, 600, 1000);
    public static readonly Image ExpiredUnattachedImage = new(WellKnownUsers.Explorer, 800, 600, 1000, DateTime.UtcNow.AddHours(-30));

    static ImageSeed()
    {
        OldFortressPhotoOnDraft = new(WellKnownUsers.Explorer, 800, 600, 1000, DateTime.UtcNow.AddHours(-30));
        OldFortressPhotoOnDraft.AttachToBlog(WellKnownUsers.Explorer);

        FortressGatePhotoOnDraft = new(WellKnownUsers.Explorer, 800, 600, 1000);
        FortressGatePhotoOnDraft.AttachToBlog(WellKnownUsers.Explorer);

        RiverbankPhotoOnPublishedBlog = new(WellKnownUsers.Explorer, 800, 600, 1000);
        RiverbankPhotoOnPublishedBlog.AttachToBlog(WellKnownUsers.Explorer);
    }

    public static Image[] All =>
    [
        OldFortressPhotoOnDraft,
        FortressGatePhotoOnDraft,
        RiverbankPhotoOnPublishedBlog,
        ExplorersUnattachedImage,
        SecondExplorersUnattachedImage,
        ExpiredUnattachedImage
    ];
}
