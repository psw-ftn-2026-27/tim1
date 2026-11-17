using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletFunding;
using Payment.Application.Wallets;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletFunding;

public class WalletFundingQueryTests : BaseIntegrationTest
{
    public WalletFundingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Customer_sees_only_their_own_wallet_transactions_and_requests()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var overview = await client.GetFromJsonAsync<WalletOverviewDto>("/api/payment/wallet", JsonOptions);

        overview!.AvailableBalance.Should().Be(WalletSeed.ExplorersWallet.AvailableBalance);
        overview.ReservedBalance.Should().Be(WalletSeed.ExplorersWallet.ReservedBalance);
        overview.Transactions.Should().BeEmpty();
        overview.Requests.Should().HaveCount(
            TopUpRequestSeed.All.Count(topUpRequest => topUpRequest.WalletId == WalletSeed.ExplorersWallet.Id));
        overview.Requests.Should().Contain(request => request.Id == TopUpRequestSeed.ExplorersPendingTopUp.Id);
        overview.Requests.Should().NotContain(request => request.Id == TopUpRequestSeed.SecondExplorersPendingTopUp.Id);
    }

    [Fact]
    public async Task Customer_without_a_wallet_sees_an_empty_wallet()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var overview = await client.GetFromJsonAsync<WalletOverviewDto>("/api/payment/wallet", JsonOptions);

        overview!.AvailableBalance.Should().Be(0);
        overview.ReservedBalance.Should().Be(0);
        overview.Transactions.Should().BeEmpty();
        overview.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Customer_sees_requests_of_both_kinds()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var overview = await client.GetFromJsonAsync<WalletOverviewDto>("/api/payment/wallet", JsonOptions);

        overview!.Requests.Should().Contain(request =>
            request.Id == TopUpRequestSeed.SecondExplorersPendingTopUp.Id
            && request.Type == WalletRequestType.TopUp);
        overview.Requests.Should().Contain(request =>
            request.Id == WithdrawalRequestSeed.SecondExplorersPendingWithdrawal.Id
            && request.Type == WalletRequestType.Withdrawal);
    }

    [Fact]
    public async Task Customer_sees_the_reason_of_a_rejected_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var overview = await client.GetFromJsonAsync<WalletOverviewDto>("/api/payment/wallet", JsonOptions);

        overview!.Requests.Should().Contain(request =>
            request.Id == TopUpRequestSeed.ExplorersRejectedTopUp.Id
            && request.RejectionReason == TopUpRequestSeed.ExplorersRejectedTopUp.RejectionReason);
    }
}
