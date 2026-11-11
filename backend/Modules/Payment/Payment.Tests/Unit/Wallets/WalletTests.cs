using FluentAssertions;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Wallets;

public class WalletTests
{
    private static Wallet CreateWallet() => new(WellKnownUsers.Explorer);

    private static Wallet CreateWalletWithReservedFunds()
    {
        var wallet = CreateWallet();
        wallet.Deposit(5000m);
        wallet.ReserveForWithdrawal(1000m);
        return wallet;
    }

    [Fact]
    public void Withdrawal_moves_its_amount_from_the_available_to_the_reserved_balance()
    {
        var wallet = CreateWalletWithReservedFunds();

        wallet.ReserveForWithdrawal(1500m);

        wallet.AvailableBalance.Should().Be(2500m);
        wallet.ReservedBalance.Should().Be(2500m);
        wallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.Reservation
            && transaction.Amount == 1500m
            && transaction.BalanceAfter == 2500m);
    }

    [Fact]
    public void Withdrawal_above_the_available_balance_is_not_reserved()
    {
        var wallet = CreateWallet();

        var reservation = () => wallet.ReserveForWithdrawal(500m);

        reservation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-500)]
    public void Withdrawal_requires_an_amount_greater_than_zero(decimal amount)
    {
        var wallet = CreateWallet();

        var reservation = () => wallet.ReserveForWithdrawal(amount);

        reservation.Should().Throw<DomainException>();
    }
}
