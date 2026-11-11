using Payment.Domain.TopUpRequests;

namespace Payment.Tests.Integration.Seeds;

internal static class TopUpRequestSeed
{
    public static readonly TopUpRequest ExplorersPendingTopUp = new(WalletSeed.ExplorersWallet.Id, 1500m);
    public static readonly TopUpRequest SecondExplorersPendingTopUp = new(WalletSeed.SecondExplorersWallet.Id, 800m);

    public static TopUpRequest[] All => [ExplorersPendingTopUp, SecondExplorersPendingTopUp];
}
