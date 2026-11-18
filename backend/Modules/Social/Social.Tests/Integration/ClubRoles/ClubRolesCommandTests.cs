using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.Clubs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubRoles;

public class ClubRolesCommandTests : BaseIntegrationTest
{
    public ClubRolesCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_transfers_ownership_and_the_new_owner_is_stored_as_the_club_owner()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.Id;

        var response = await client.PutAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.SecondExplorer}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.OwnerId.Should().Be(WellKnownUsers.SecondExplorer);
        stored.Members.Should().ContainSingle(member => member.Role == ClubRole.Owner)
            .Which.UserId.Should().Be(WellKnownUsers.SecondExplorer);
        stored.Members.Should().ContainSingle(member => member.UserId == WellKnownUsers.Explorer)
            .Which.Role.Should().Be(ClubRole.Member);
    }

    [Fact]
    public async Task Anonymous_user_cannot_promote_a_member_to_moderator()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.Id}/members/{WellKnownUsers.SecondExplorer}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_promote_a_member_to_moderator()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.Id}/members/{WellKnownUsers.SecondExplorer}/promote", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_transfer_ownership()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.Id}/members/{WellKnownUsers.SecondExplorer}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_transfer_ownership()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.Id}/members/{WellKnownUsers.SecondExplorer}/transfer-ownership", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_removes_a_member_and_the_member_is_no_longer_stored_in_the_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.Id;

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{clubId}/members/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Members.Should().NotContain(member => member.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_member()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{ClubSeed.ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.Id}/members/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync(
            $"/api/social/clubs/{ClubSeed.ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest.Id}/members/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
