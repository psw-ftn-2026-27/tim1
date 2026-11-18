using Games.Domain.Images;

namespace Games.Tests.Integration.Seeds;

internal static class ImageSeed
{
    public static readonly Image FortressGatePhoto = new(800, 600, 1000);
    public static readonly Image ClockTowerPhoto = new(800, 600, 1000);
    public static readonly Image CathedralPhoto = new(800, 600, 1000);
    public static readonly Image ParkFountainPhoto = new(800, 600, 1000);
    public static readonly Image QuayBridgePhoto = new(800, 600, 1000);
    public static readonly Image LookoutPhotoNotYetOnAChallenge = new(800, 600, 1000);
    public static readonly Image UniversityPhotoNotYetOnAChallenge = new(800, 600, 1000);

    public static Image[] All =>
    [
        FortressGatePhoto,
        ClockTowerPhoto,
        CathedralPhoto,
        ParkFountainPhoto,
        QuayBridgePhoto,
        LookoutPhotoNotYetOnAChallenge,
        UniversityPhotoNotYetOnAChallenge
    ];
}
