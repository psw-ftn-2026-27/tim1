using Payment.Domain.GiftCards;

namespace Payment.Tests.Integration.Seeds;

internal static class GiftCardSeed
{
    public static readonly GiftCard ExplorersGiftCardForSecondExplorer = new(DemoUsers.Explorer, DemoUsers.SecondExplorer, 100m, "Enjoy the tours!");
    public static readonly GiftCard SecondExplorersGiftCardForExplorer = new(DemoUsers.SecondExplorer, DemoUsers.Explorer, 250m, "Happy birthday!");
    public static readonly GiftCard SecondExplorersGiftCardForThirdExplorer = new(DemoUsers.SecondExplorer, DemoUsers.ThirdExplorer, 50m, "Thank you!");

    public static GiftCard[] All =>
        [ExplorersGiftCardForSecondExplorer, SecondExplorersGiftCardForExplorer, SecondExplorersGiftCardForThirdExplorer];
}
