using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.Clubs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubMembership;

public class ClubMembershipCommandTests : BaseIntegrationTest
{
    public ClubMembershipCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_sends_a_membership_request_that_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var clubId = ClubSeed.HikersClub.Id;
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.Clubs.Single(club => club.Id == clubId).MembershipRequests.Count;

        var response = await client.PostAsync($"/api/social/clubs/{clubId}/membership-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.MembershipRequests.Should().HaveCount(requestCountBefore + 1);
        stored.MembershipRequests.Should().Contain(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_request_membership()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}/membership-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_request_membership()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.HikersClub.Id}/membership-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_withdraw_a_membership_request()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{ClubSeed.RunnersClubWithSecondExplorersPendingRequest.Id}/membership-requests/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_withdraw_a_membership_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{ClubSeed.RunnersClubWithSecondExplorersPendingRequest.Id}/membership-requests/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_accepts_a_request_and_the_requester_becomes_a_member_without_a_pending_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.RunnersClubWithSecondExplorersPendingRequest.Id;

        var response = await client.PutAsync(
            $"/api/social/clubs/{clubId}/membership-requests/{WellKnownUsers.SecondExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Members.Should().ContainSingle(member => member.UserId == WellKnownUsers.SecondExplorer)
            .Which.Role.Should().Be(ClubRole.Member);
        stored.MembershipRequests.Should().NotContain(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_accept_a_membership_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.RunnersClubWithSecondExplorersPendingRequest.Id}/membership-requests/{WellKnownUsers.SecondExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_accept_a_membership_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.RunnersClubWithSecondExplorersPendingRequest.Id}/membership-requests/{WellKnownUsers.SecondExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reject_a_membership_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.RunnersClubWithSecondExplorersPendingRequest.Id}/membership-requests/{WellKnownUsers.SecondExplorer}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_reject_a_membership_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.RunnersClubWithSecondExplorersPendingRequest.Id}/membership-requests/{WellKnownUsers.SecondExplorer}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
