using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorersWallet = new(WellKnownUsers.Explorer);
    public static readonly Wallet SecondExplorersWallet = new(WellKnownUsers.SecondExplorer);

    public static Wallet[] All => [ExplorersWallet, SecondExplorersWallet];
}
