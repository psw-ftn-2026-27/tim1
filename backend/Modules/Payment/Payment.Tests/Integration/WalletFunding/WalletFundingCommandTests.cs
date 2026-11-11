using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletFunding;
using Payment.Domain.Requests;
using Payment.Domain.TopUpRequests;
using Payment.Domain.Wallets;
using Payment.Domain.WithdrawalRequests;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletFunding;

public class WalletFundingCommandTests : BaseIntegrationTest
{
    public WalletFundingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task First_top_up_request_of_a_customer_without_a_wallet_opens_an_empty_wallet()
    {
        var newCustomer = Guid.NewGuid();
        var client = Factory.CreateClientFor(newCustomer, "explorer");
        var request = new RequestTopUpDto(2500m);

        var response = await client.PostAsJsonAsync("/api/payment/wallet/top-up-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.OwnerId == newCustomer);
        storedWallet.AvailableBalance.Should().Be(0);
        storedWallet.ReservedBalance.Should().Be(0);
        storedWallet.Transactions.Should().BeEmpty();
        var storedRequest = assertContext.TopUpRequests.Single(topUpRequest => topUpRequest.WalletId == storedWallet.Id);
        storedRequest.Amount.Should().Be(2500m);
        storedRequest.Status.Should().Be(WalletRequestStatus.Pending);
    }

    [Fact]
    public async Task Top_up_request_of_a_customer_with_a_wallet_leaves_both_balances_untouched()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new RequestTopUpDto(1200m);
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var walletCountBefore = arrangeContext.Wallets.Count();
        var requestCountBefore = arrangeContext.TopUpRequests.Count();

        var response = await client.PostAsJsonAsync("/api/payment/wallet/top-up-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Count().Should().Be(walletCountBefore);
        assertContext.TopUpRequests.Count().Should().Be(requestCountBefore + 1);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ExplorersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(WalletSeed.ExplorersWallet.AvailableBalance);
        storedWallet.ReservedBalance.Should().Be(WalletSeed.ExplorersWallet.ReservedBalance);
        storedWallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Top_up_request_with_an_amount_of_zero_is_not_recorded()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new RequestTopUpDto(0);
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var requestCountBefore = arrangeContext.TopUpRequests.Count();

        var response = await client.PostAsJsonAsync("/api/payment/wallet/top-up-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.TopUpRequests.Count().Should().Be(requestCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_request_a_top_up()
    {
        var client = Factory.CreateClient();
        var request = new RequestTopUpDto(2500m);

        var response = await client.PostAsJsonAsync("/api/payment/wallet/top-up-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_request_a_top_up()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new RequestTopUpDto(2500m);

        var response = await client.PostAsJsonAsync("/api/payment/wallet/top-up-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_reserves_their_available_funds_by_requesting_a_withdrawal()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new RequestWithdrawalDto(1500m);
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var requestCountBefore = arrangeContext.WithdrawalRequests.Count();
        var transactionCountBefore = arrangeContext.Wallets
            .Single(wallet => wallet.Id == WalletSeed.SecondExplorersWallet.Id).Transactions.Count;
        var expectedAvailableBalance = WalletSeed.SecondExplorersWallet.AvailableBalance - 1500m;

        var response = await client.PostAsJsonAsync("/api/payment/wallet/withdrawal-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.WithdrawalRequests.Count().Should().Be(requestCountBefore + 1);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.SecondExplorersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(expectedAvailableBalance);
        storedWallet.ReservedBalance.Should().Be(WalletSeed.SecondExplorersWallet.ReservedBalance + 1500m);
        storedWallet.Transactions.Should().HaveCount(transactionCountBefore + 1);
        storedWallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.Reservation
            && transaction.Amount == 1500m
            && transaction.BalanceAfter == expectedAvailableBalance);
        var storedRequest = assertContext.WithdrawalRequests.Single(withdrawalRequest =>
            withdrawalRequest.WalletId == WalletSeed.SecondExplorersWallet.Id && withdrawalRequest.Amount == 1500m);
        storedRequest.Status.Should().Be(WalletRequestStatus.Pending);
    }

    [Fact]
    public async Task Withdrawal_request_above_the_available_balance_is_not_recorded()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new RequestWithdrawalDto(500m);
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var requestCountBefore = arrangeContext.WithdrawalRequests.Count();

        var response = await client.PostAsJsonAsync("/api/payment/wallet/withdrawal-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.WithdrawalRequests.Count().Should().Be(requestCountBefore);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ExplorersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(WalletSeed.ExplorersWallet.AvailableBalance);
        storedWallet.ReservedBalance.Should().Be(WalletSeed.ExplorersWallet.ReservedBalance);
        storedWallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Author_without_a_wallet_cannot_request_a_withdrawal()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        var request = new RequestWithdrawalDto(500m);
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var walletCountBefore = arrangeContext.Wallets.Count();
        var requestCountBefore = arrangeContext.WithdrawalRequests.Count();

        var response = await client.PostAsJsonAsync("/api/payment/wallet/withdrawal-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Count().Should().Be(walletCountBefore);
        assertContext.WithdrawalRequests.Count().Should().Be(requestCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_request_a_withdrawal()
    {
        var client = Factory.CreateClient();
        var request = new RequestWithdrawalDto(500m);

        var response = await client.PostAsJsonAsync("/api/payment/wallet/withdrawal-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_request_a_withdrawal()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new RequestWithdrawalDto(500m);

        var response = await client.PostAsJsonAsync("/api/payment/wallet/withdrawal-requests", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
