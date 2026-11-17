using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowManagement;

public class FollowManagementCommandTests : BaseIntegrationTest
{
    public FollowManagementCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_removes_a_follower_and_only_that_direction_of_following_ends()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");
        var requestId = FollowRequestSeed.DemoSecondExplorerAcceptedToExplorer.Id;
        var oppositeRequestId = FollowRequestSeed.DemoExplorerAcceptedToSecondExplorer.Id;

        var response = await client.DeleteAsync($"/api/social/followers/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests.Any(request => request.Id == requestId).Should().BeFalse();
        assertContext.FollowRequests.Any(request => request.Id == oppositeRequestId).Should().BeTrue();
    }

    [Fact]
    public async Task Explorer_cannot_remove_a_follower_whose_request_is_still_pending()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");
        var requestId = FollowRequestSeed.DemoSecondAdministratorPendingToExplorer.Id;

        var response = await client.DeleteAsync($"/api/social/followers/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests.Any(request => request.Id == requestId).Should().BeTrue();
    }

    [Fact]
    public async Task Explorer_cannot_remove_a_follower_through_a_request_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/followers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_follower()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/followers/{FollowRequestSeed.DemoSecondExplorerAcceptedToExplorer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_follower()
    {
        var client = Factory.CreateClientFor(DemoUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/followers/{FollowRequestSeed.DemoSecondExplorerAcceptedToExplorer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Explorer_stops_following_a_user_and_only_that_direction_of_following_ends()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");
        var requestId = FollowRequestSeed.DemoExplorerAcceptedToSecondExplorer.Id;
        var oppositeRequestId = FollowRequestSeed.DemoSecondExplorerAcceptedToExplorer.Id;

        var response = await client.DeleteAsync($"/api/social/following/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests.Any(request => request.Id == requestId).Should().BeFalse();
        assertContext.FollowRequests.Any(request => request.Id == oppositeRequestId).Should().BeTrue();
    }

    [Fact]
    public async Task Explorer_cannot_stop_following_a_user_whose_request_is_still_pending()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");
        var requestId = FollowRequestSeed.DemoExplorerPendingToThirdExplorer.Id;

        var response = await client.DeleteAsync($"/api/social/following/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests.Any(request => request.Id == requestId).Should().BeTrue();
    }

    [Fact]
    public async Task Explorer_cannot_stop_following_through_a_request_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/following/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_user_cannot_stop_following_a_user()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/following/{FollowRequestSeed.DemoExplorerAcceptedToSecondExplorer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_stop_following_a_user()
    {
        var client = Factory.CreateClientFor(DemoUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/following/{FollowRequestSeed.DemoExplorerAcceptedToSecondExplorer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
