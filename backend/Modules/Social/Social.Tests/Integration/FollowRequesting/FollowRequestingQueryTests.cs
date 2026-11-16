using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.FollowRequesting;
using Social.Domain.FollowRequests;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowRequesting;

public class FollowRequestingQueryTests : BaseIntegrationTest
{
    public FollowRequestingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_lists_only_the_follow_requests_they_sent()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var requests = await client.GetFromJsonAsync<List<FollowRequestDto>>("/api/social/follow-requests/sent", JsonOptions);

        requests.Should().HaveCount(FollowRequestSeed.All.Count(request => request.SenderId == WellKnownUsers.Explorer));
        requests.Should().Contain(request => request.Id == FollowRequestSeed.ExplorerPendingToSecondExplorer.Id);
        requests.Should().NotContain(request => request.Id == FollowRequestSeed.AdministratorPendingToExplorer.Id);
    }

    [Fact]
    public async Task Search_leaves_out_the_explorer_the_users_they_follow_and_the_users_with_a_pending_request()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var users = await client.GetFromJsonAsync<PageResult<UserToFollowDto>>("/api/social/follow-requests/search", JsonOptions);

        users!.Items.Should().NotContain(user => user.Id == DemoUsers.Explorer);
        users.Items.Should().NotContain(user => user.Id == DemoUsers.SecondExplorer);
        users.Items.Should().NotContain(user => user.Id == DemoUsers.ThirdExplorer);
        users.Items.Should().Contain(user => user.Id == DemoUsers.SecondAdministrator);
    }

    [Fact]
    public async Task Search_includes_a_user_whose_earlier_request_was_rejected()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var users = await client.GetFromJsonAsync<PageResult<UserToFollowDto>>("/api/social/follow-requests/search", JsonOptions);

        users!.Items.Should().Contain(user => user.Id == DemoUsers.Administrator);
    }

    [Fact]
    public async Task Explorer_lists_only_the_follow_requests_they_received_with_their_state()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var requests = await client.GetFromJsonAsync<PageResult<ReceivedFollowRequestDto>>("/api/social/follow-requests/received", JsonOptions);

        requests!.TotalCount.Should().Be(FollowRequestSeed.All.Count(request => request.RecipientId == WellKnownUsers.Explorer));
        requests.Items.Should().NotContain(request => request.Id == FollowRequestSeed.ExplorerPendingToSecondExplorer.Id);
        requests.Items.Single(request => request.Id == FollowRequestSeed.AdministratorPendingToExplorer.Id)
            .Status.Should().Be(FollowRequestStatus.Pending);
        requests.Items.Single(request => request.Id == FollowRequestSeed.SecondExplorerRejectedToExplorer.Id)
            .Status.Should().Be(FollowRequestStatus.Rejected);
    }

    [Fact]
    public async Task Pending_received_count_includes_only_requests_that_are_pending()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var count = await client.GetFromJsonAsync<int>("/api/social/follow-requests/received/pending-count", JsonOptions);

        count.Should().Be(FollowRequestSeed.All.Count(request =>
            request.RecipientId == WellKnownUsers.Explorer && request.Status == FollowRequestStatus.Pending));
    }

    [Fact]
    public async Task Anonymous_user_cannot_list_received_follow_requests()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/social/follow-requests/received");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_read_the_pending_received_count()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/social/follow-requests/received/pending-count");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_list_sent_follow_requests()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/social/follow-requests/sent");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_search_users_to_follow()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/social/follow-requests/search");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
