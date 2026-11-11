using Payment.Domain.WithdrawalRequests;

namespace Payment.Tests.Integration.Seeds;

internal static class WithdrawalRequestSeed
{
    public static readonly WithdrawalRequest SecondExplorersPendingWithdrawal = new(WalletSeed.SecondExplorersWallet.Id, 1000m);

    public static WithdrawalRequest[] All => [SecondExplorersPendingWithdrawal];
}
