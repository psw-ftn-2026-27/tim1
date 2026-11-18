using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorersWallet = new(WellKnownUsers.Explorer);
    public static readonly Wallet SecondExplorersWallet;

    /// <summary>
    /// The empty wallet of the owner of <see cref="ProductSeed.DemoExplorersTour"/>, so a sale
    /// of that tour lands on a wallet that already exists.
    /// </summary>
    public static readonly Wallet ProductOwnersWallet = new(DemoUsers.Explorer);

    /// <summary>
    /// The wallet of the owner of <see cref="ShoppingCartSeed.BuyersCart"/>, holding more than
    /// that cart comes to, so the whole cart can be paid for.
    /// </summary>
    public static readonly Wallet BuyersWallet;

    static WalletSeed()
    {
        SecondExplorersWallet = new Wallet(WellKnownUsers.SecondExplorer);
        SecondExplorersWallet.Deposit(5000m);
        SecondExplorersWallet.ReserveForWithdrawal(1000m);

        BuyersWallet = new Wallet(DemoUsers.SecondExplorer);
        BuyersWallet.Deposit(5000m);
    }

    public static Wallet[] All => [ExplorersWallet, SecondExplorersWallet, ProductOwnersWallet, BuyersWallet];
}
