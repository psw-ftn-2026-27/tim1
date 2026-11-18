using FluentAssertions;
using Payment.Domain.Products;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Wallets;

public class WalletTests
{
    private const string PaidProductName = "Walk around the Petrovaradin fortress";
    private static readonly Guid Seller = WellKnownUsers.SecondExplorer;

    private static Wallet CreateWallet() => new(WellKnownUsers.Explorer);

    private static ProductReference CreatePaidProduct() =>
        new(ProductType.Tour, Guid.Parse("30000000-0000-0000-0000-000000000001"));

    private static Wallet CreateWalletWithReservedFunds()
    {
        var wallet = CreateWallet();
        wallet.Deposit(5000m);
        wallet.ReserveForWithdrawal(1000m);
        return wallet;
    }

    [Fact]
    public void Top_up_increases_the_available_balance_and_records_a_transaction()
    {
        var wallet = CreateWallet();

        wallet.Deposit(2500m);

        wallet.AvailableBalance.Should().Be(2500m);
        wallet.ReservedBalance.Should().Be(0);
        wallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.TopUp
            && transaction.Amount == 2500m
            && transaction.BalanceAfter == 2500m);
    }

    [Fact]
    public void Approved_payout_lowers_the_reserved_balance_and_leaves_the_available_balance()
    {
        var wallet = CreateWalletWithReservedFunds();

        wallet.PayOutReserved(1000m);

        wallet.AvailableBalance.Should().Be(4000m);
        wallet.ReservedBalance.Should().Be(0);
        wallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.Payout
            && transaction.Amount == 1000m
            && transaction.BalanceAfter == 4000m);
    }

    [Fact]
    public void Rejected_payout_returns_its_amount_from_the_reserved_to_the_available_balance()
    {
        var wallet = CreateWalletWithReservedFunds();

        wallet.ReturnReserved(1000m);

        wallet.AvailableBalance.Should().Be(5000m);
        wallet.ReservedBalance.Should().Be(0);
        wallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.ReservationReturn
            && transaction.Amount == 1000m
            && transaction.BalanceAfter == 5000m);
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

    [Fact]
    public void Payment_lowers_the_available_balance_and_records_the_product_it_paid_for()
    {
        var wallet = CreateWalletWithReservedFunds();

        wallet.PayFor(CreatePaidProduct(), PaidProductName, 1200m, Seller);

        wallet.AvailableBalance.Should().Be(2800m);
        wallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.Purchase
            && transaction.Amount == 1200m
            && transaction.BalanceAfter == 2800m
            && transaction.ProductName == PaidProductName
            && transaction.Reference == CreatePaidProduct());
    }

    [Fact]
    public void Payment_above_the_available_balance_is_refused()
    {
        var wallet = CreateWalletWithReservedFunds();

        var payment = () => wallet.PayFor(CreatePaidProduct(), PaidProductName, 5000m, Seller);

        payment.Should().Throw<DomainException>();
    }

    [Fact]
    public void Payment_leaves_the_reserved_funds_untouched()
    {
        var wallet = CreateWalletWithReservedFunds();

        wallet.PayFor(CreatePaidProduct(), PaidProductName, 1200m, Seller);

        wallet.ReservedBalance.Should().Be(1000m);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-500)]
    public void Payment_of_a_negative_amount_is_refused(decimal price)
    {
        var wallet = CreateWalletWithReservedFunds();

        var payment = () => wallet.PayFor(CreatePaidProduct(), PaidProductName, price, Seller);

        payment.Should().Throw<DomainException>();
    }

    [Fact]
    public void Free_product_is_paid_for_without_changing_the_balance()
    {
        var wallet = CreateWalletWithReservedFunds();

        wallet.PayFor(CreatePaidProduct(), PaidProductName, 0m, Seller);

        wallet.AvailableBalance.Should().Be(4000m);
        wallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.Purchase
            && transaction.Amount == 0m
            && transaction.ProductName == PaidProductName);
    }

    [Fact]
    public void Payment_for_a_product_of_the_wallets_own_owner_is_refused()
    {
        var wallet = CreateWalletWithReservedFunds();

        var payment = () => wallet.PayFor(CreatePaidProduct(), PaidProductName, 1200m, WellKnownUsers.Explorer);

        payment.Should().Throw<DomainException>();
        wallet.AvailableBalance.Should().Be(4000m);
        wallet.ReservedBalance.Should().Be(1000m);
        wallet.Transactions.Should().NotContain(transaction => transaction.Type == TransactionType.Purchase);
    }

    [Fact]
    public void Received_payment_increases_the_available_balance_and_records_the_product_it_sold()
    {
        var wallet = CreateWalletWithReservedFunds();

        wallet.ReceivePayment(CreatePaidProduct(), PaidProductName, 1200m);

        wallet.AvailableBalance.Should().Be(5200m);
        wallet.ReservedBalance.Should().Be(1000m);
        wallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.Sale
            && transaction.Amount == 1200m
            && transaction.BalanceAfter == 5200m
            && transaction.ProductName == PaidProductName
            && transaction.Reference == CreatePaidProduct());
    }
}
