namespace Payment.Tests.Integration.Seeds;

internal static class PaymentSeed
{
    public static object[] All => [.. WalletSeed.All, .. TopUpRequestSeed.All];
}
