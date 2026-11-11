using Payment.Domain.TopUpRequests;

namespace Payment.Tests.Integration.Seeds;

internal static class TopUpRequestSeed
{
    public static readonly TopUpRequest ExplorersPendingTopUp = new(WalletSeed.ExplorersWallet.Id, 1500m);
    public static readonly TopUpRequest SecondExplorersPendingTopUp = new(WalletSeed.SecondExplorersWallet.Id, 800m);
    public static readonly TopUpRequest ExplorersRejectedTopUp;

    static TopUpRequestSeed()
    {
        ExplorersRejectedTopUp = new TopUpRequest(WalletSeed.ExplorersWallet.Id, 300m);
        ExplorersRejectedTopUp.Reject("The payment proof does not match the amount.");
    }

    public static TopUpRequest[] All =>
        [ExplorersPendingTopUp, SecondExplorersPendingTopUp, ExplorersRejectedTopUp];
}
