using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletRequestReview;
using Payment.Domain.TopUpRequests;
using Payment.Domain.Wallets;
using Payment.Domain.WithdrawalRequests;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletRequestReview;

public class WalletRequestReviewCommandTests : BaseIntegrationTest
{
    public WalletRequestReviewCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_approves_a_top_up_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var expectedAvailableBalance =
            WalletSeed.ExplorersWallet.AvailableBalance + TopUpRequestSeed.ExplorersPendingTopUp.Amount;

        var response = await client.PostAsync(
            $"/api/payment/wallet/requests/top-ups/{TopUpRequestSeed.ExplorersPendingTopUp.Id}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedRequest = assertContext.TopUpRequests
            .Single(topUpRequest => topUpRequest.Id == TopUpRequestSeed.ExplorersPendingTopUp.Id);
        storedRequest.Status.Should().Be(TopUpRequestStatus.Approved);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ExplorersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(expectedAvailableBalance);
        storedWallet.ReservedBalance.Should().Be(WalletSeed.ExplorersWallet.ReservedBalance);
        storedWallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.TopUp
            && transaction.Amount == TopUpRequestSeed.ExplorersPendingTopUp.Amount
            && transaction.BalanceAfter == expectedAvailableBalance);
    }

    [Fact]
    public async Task Administrator_rejects_a_top_up_request_with_a_reason()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var rejection = new RejectRequestDto("The payment proof is missing.");

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallet/requests/top-ups/{TopUpRequestSeed.ExplorersPendingTopUp.Id}/reject", rejection);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedRequest = assertContext.TopUpRequests
            .Single(topUpRequest => topUpRequest.Id == TopUpRequestSeed.ExplorersPendingTopUp.Id);
        storedRequest.Status.Should().Be(TopUpRequestStatus.Rejected);
        storedRequest.RejectionReason.Should().Be(rejection.Reason);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ExplorersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(WalletSeed.ExplorersWallet.AvailableBalance);
        storedWallet.ReservedBalance.Should().Be(WalletSeed.ExplorersWallet.ReservedBalance);
        storedWallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Administrator_approves_a_withdrawal_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var paidOutAmount = WithdrawalRequestSeed.SecondExplorersPendingWithdrawal.Amount;

        var response = await client.PostAsync(
            $"/api/payment/wallet/requests/withdrawals/{WithdrawalRequestSeed.SecondExplorersPendingWithdrawal.Id}/approve",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedRequest = assertContext.WithdrawalRequests
            .Single(withdrawalRequest =>
                withdrawalRequest.Id == WithdrawalRequestSeed.SecondExplorersPendingWithdrawal.Id);
        storedRequest.Status.Should().Be(WithdrawalRequestStatus.Approved);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.SecondExplorersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(WalletSeed.SecondExplorersWallet.AvailableBalance);
        storedWallet.ReservedBalance.Should().Be(WalletSeed.SecondExplorersWallet.ReservedBalance - paidOutAmount);
        storedWallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.Payout
            && transaction.Amount == paidOutAmount
            && transaction.BalanceAfter == WalletSeed.SecondExplorersWallet.AvailableBalance);
    }

    [Fact]
    public async Task Administrator_rejects_a_withdrawal_request_with_a_reason()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var rejection = new RejectRequestDto("The bank account could not be verified.");
        var returnedAmount = WithdrawalRequestSeed.SecondExplorersPendingWithdrawal.Amount;
        var expectedAvailableBalance = WalletSeed.SecondExplorersWallet.AvailableBalance + returnedAmount;

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallet/requests/withdrawals/{WithdrawalRequestSeed.SecondExplorersPendingWithdrawal.Id}/reject",
            rejection);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedRequest = assertContext.WithdrawalRequests
            .Single(withdrawalRequest =>
                withdrawalRequest.Id == WithdrawalRequestSeed.SecondExplorersPendingWithdrawal.Id);
        storedRequest.Status.Should().Be(WithdrawalRequestStatus.Rejected);
        storedRequest.RejectionReason.Should().Be(rejection.Reason);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.SecondExplorersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(expectedAvailableBalance);
        storedWallet.ReservedBalance.Should().Be(WalletSeed.SecondExplorersWallet.ReservedBalance - returnedAmount);
        storedWallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.ReservationReturn
            && transaction.Amount == returnedAmount
            && transaction.BalanceAfter == expectedAvailableBalance);
    }

    [Fact]
    public async Task Request_that_has_been_decided_cannot_be_decided_again()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/payment/wallet/requests/top-ups/{TopUpRequestSeed.ExplorersRejectedTopUp.Id}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedRequest = assertContext.TopUpRequests
            .Single(topUpRequest => topUpRequest.Id == TopUpRequestSeed.ExplorersRejectedTopUp.Id);
        storedRequest.Status.Should().Be(TopUpRequestStatus.Rejected);
        storedRequest.RejectionReason.Should().Be(TopUpRequestSeed.ExplorersRejectedTopUp.RejectionReason);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ExplorersWallet.Id);
        storedWallet.AvailableBalance.Should().Be(WalletSeed.ExplorersWallet.AvailableBalance);
        storedWallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Decision_on_a_request_that_does_not_exist_is_not_possible()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var approvedCountBefore = arrangeContext.TopUpRequests
            .Count(topUpRequest => topUpRequest.Status == TopUpRequestStatus.Approved);

        var response = await client.PostAsync(
            $"/api/payment/wallet/requests/top-ups/{Guid.NewGuid()}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.TopUpRequests
            .Count(topUpRequest => topUpRequest.Status == TopUpRequestStatus.Approved)
            .Should().Be(approvedCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_decide_on_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/payment/wallet/requests/top-ups/{TopUpRequestSeed.ExplorersPendingTopUp.Id}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Customer_cannot_decide_on_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/payment/wallet/requests/top-ups/{TopUpRequestSeed.ExplorersPendingTopUp.Id}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
