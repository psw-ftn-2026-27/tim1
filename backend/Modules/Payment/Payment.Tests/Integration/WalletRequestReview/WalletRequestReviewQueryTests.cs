using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletRequestReview;
using Payment.Application.Wallets;
using Payment.Domain.Requests;
using Payment.Domain.TopUpRequests;
using Payment.Domain.WithdrawalRequests;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletRequestReview;

public class WalletRequestReviewQueryTests : BaseIntegrationTest
{
    public WalletRequestReviewQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_pending_requests_in_one_list_and_decided_requests_in_the_other()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var pendingSeedCount =
            TopUpRequestSeed.All.Count(topUpRequest => topUpRequest.Status == WalletRequestStatus.Pending)
            + WithdrawalRequestSeed.All.Count(withdrawalRequest =>
                withdrawalRequest.Status == WalletRequestStatus.Pending);
        var decidedSeedCount =
            TopUpRequestSeed.All.Count(topUpRequest => topUpRequest.Status != WalletRequestStatus.Pending)
            + WithdrawalRequestSeed.All.Count(withdrawalRequest =>
                withdrawalRequest.Status != WalletRequestStatus.Pending);

        var requests = await client.GetFromJsonAsync<WalletRequestsDto>(
            "/api/payment/wallet/requests", JsonOptions);

        requests!.Pending.Should().HaveCount(pendingSeedCount);
        requests.Pending.Should().Contain(request =>
            request.Id == TopUpRequestSeed.ExplorersPendingTopUp.Id
            && request.Type == WalletRequestType.TopUp
            && request.SubmitterId == WellKnownUsers.Explorer
            && request.Amount == TopUpRequestSeed.ExplorersPendingTopUp.Amount
            && request.Status == WalletRequestStatus.Pending
            && request.RejectionReason == null);
        requests.Pending.Should().Contain(request =>
            request.Id == WithdrawalRequestSeed.SecondExplorersPendingWithdrawal.Id
            && request.Type == WalletRequestType.Withdrawal
            && request.SubmitterId == WellKnownUsers.SecondExplorer
            && request.Amount == WithdrawalRequestSeed.SecondExplorersPendingWithdrawal.Amount);
        requests.Decided.Should().HaveCount(decidedSeedCount);
        requests.Decided.Should().Contain(request =>
            request.Id == TopUpRequestSeed.ExplorersRejectedTopUp.Id
            && request.Status == WalletRequestStatus.Rejected
            && request.RejectionReason == TopUpRequestSeed.ExplorersRejectedTopUp.RejectionReason);
    }
}
