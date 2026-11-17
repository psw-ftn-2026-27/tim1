using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorersWallet = new(WellKnownUsers.Explorer);
    public static readonly Wallet SecondExplorersWallet;
    public static readonly Wallet FundedDemoExplorersWallet;
    public static readonly Wallet EmptyDemoThirdExplorersWallet = new(DemoUsers.ThirdExplorer);

    static WalletSeed()
    {
        SecondExplorersWallet = new Wallet(WellKnownUsers.SecondExplorer);
        SecondExplorersWallet.Deposit(5000m);
        SecondExplorersWallet.ReserveForWithdrawal(1000m);

        FundedDemoExplorersWallet = new Wallet(DemoUsers.Explorer);
        FundedDemoExplorersWallet.Deposit(500m);
    }

    public static Wallet[] All =>
        [ExplorersWallet, SecondExplorersWallet, FundedDemoExplorersWallet, EmptyDemoThirdExplorersWallet];
}
