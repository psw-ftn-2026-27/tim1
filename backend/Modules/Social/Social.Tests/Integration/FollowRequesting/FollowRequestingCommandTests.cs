using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.FollowRequests;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowRequesting;

public class FollowRequestingCommandTests : BaseIntegrationTest
{
    public FollowRequestingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sends_a_follow_request_that_is_stored_as_pending()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.FollowRequests.Count();

        var response = await client.PostAsync($"/api/social/follow-requests/{WellKnownUsers.Administrator}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests.Count().Should().Be(requestCountBefore + 1);
        var stored = assertContext.FollowRequests.Single(request =>
            request.SenderId == WellKnownUsers.Explorer && request.RecipientId == WellKnownUsers.Administrator);
        stored.Status.Should().Be(FollowRequestStatus.Pending);
    }

    [Fact]
    public async Task Explorer_cannot_send_a_second_request_while_the_first_is_pending()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.FollowRequests.Count();

        var response = await client.PostAsync($"/api/social/follow-requests/{WellKnownUsers.SecondExplorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests.Count().Should().Be(requestCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_send_a_follow_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/follow-requests/{WellKnownUsers.Administrator}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_send_a_follow_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/social/follow-requests/{WellKnownUsers.Explorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Explorer_deletes_their_pending_follow_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var requestId = FollowRequestSeed.ExplorerPendingToSecondExplorer.Id;

        var response = await client.DeleteAsync($"/api/social/follow-requests/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests.Any(request => request.Id == requestId).Should().BeFalse();
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_follow_request_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.FollowRequests.Count();

        var response = await client.DeleteAsync($"/api/social/follow-requests/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.FollowRequests.Count().Should().Be(requestCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_follow_request()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/follow-requests/{FollowRequestSeed.ExplorerPendingToSecondExplorer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_delete_a_follow_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/follow-requests/{FollowRequestSeed.AdministratorPendingToExplorer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
