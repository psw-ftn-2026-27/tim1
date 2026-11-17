using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Social.Application.FollowManagement;
using Social.Domain.FollowRequests;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowManagement;

public class FollowManagementQueryTests : BaseIntegrationTest
{
    public FollowManagementQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_lists_only_accepted_followers_and_sees_which_of_them_they_follow()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var followers = await client.GetFromJsonAsync<PageResult<FollowerDto>>("/api/social/followers", JsonOptions);

        followers!.TotalCount.Should().Be(FollowRequestSeed.All.Count(request =>
            request.RecipientId == DemoUsers.Explorer && request.Status == FollowRequestStatus.Accepted));
        followers.Items.Should().NotContain(follower => follower.Id == FollowRequestSeed.DemoSecondAdministratorPendingToExplorer.Id);
        followers.Items.Single(follower => follower.Id == FollowRequestSeed.DemoSecondExplorerAcceptedToExplorer.Id)
            .IsFollowedByUser.Should().BeTrue();
        followers.Items.Single(follower => follower.Id == FollowRequestSeed.DemoThirdExplorerAcceptedToExplorer.Id)
            .IsFollowedByUser.Should().BeFalse();
        followers.Items.Single(follower => follower.Id == FollowRequestSeed.DemoThirdExplorerAcceptedToExplorer.Id)
            .DisplayName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Follower_with_a_pending_request_from_the_explorer_is_marked_as_requested()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var followers = await client.GetFromJsonAsync<PageResult<FollowerDto>>("/api/social/followers", JsonOptions);

        var follower = followers!.Items.Single(item => item.Id == FollowRequestSeed.DemoThirdExplorerAcceptedToExplorer.Id);
        follower.IsFollowedByUser.Should().BeFalse();
        follower.HasPendingRequest.Should().BeTrue();
        followers.Items.Single(item => item.Id == FollowRequestSeed.DemoSecondExplorerAcceptedToExplorer.Id)
            .HasPendingRequest.Should().BeFalse();
    }

    [Fact]
    public async Task Explorer_lists_only_the_accepted_requests_they_sent_as_the_users_they_follow()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var following = await client.GetFromJsonAsync<PageResult<FollowingDto>>("/api/social/following", JsonOptions);

        following!.TotalCount.Should().Be(FollowRequestSeed.All.Count(request =>
            request.SenderId == DemoUsers.Explorer && request.Status == FollowRequestStatus.Accepted));
        following.Items.Should().Contain(followed => followed.Id == FollowRequestSeed.DemoExplorerAcceptedToSecondExplorer.Id);
        following.Items.Should().NotContain(followed => followed.Id == FollowRequestSeed.DemoExplorerPendingToThirdExplorer.Id);
        following.Items.Should().NotContain(followed => followed.Id == FollowRequestSeed.DemoExplorerRejectedToAdministrator.Id);
        following.Items.Should().NotContain(followed => followed.Id == FollowRequestSeed.DemoSecondExplorerAcceptedToExplorer.Id);
    }

    [Fact]
    public async Task Follow_counts_include_only_accepted_requests_in_both_directions()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var counts = await client.GetFromJsonAsync<FollowCountsDto>("/api/social/follow-counts", JsonOptions);

        counts!.FollowerCount.Should().Be(FollowRequestSeed.All.Count(request =>
            request.RecipientId == DemoUsers.Explorer && request.Status == FollowRequestStatus.Accepted));
        counts.FollowingCount.Should().Be(FollowRequestSeed.All.Count(request =>
            request.SenderId == DemoUsers.Explorer && request.Status == FollowRequestStatus.Accepted));
    }

    [Fact]
    public async Task Anonymous_user_cannot_list_followers()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/social/followers");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_list_the_users_they_follow()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/social/following");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_read_follow_counts()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/social/follow-counts");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
