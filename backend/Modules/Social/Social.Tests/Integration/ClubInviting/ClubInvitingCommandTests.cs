using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.Clubs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubInviting;

public class ClubInvitingCommandTests : BaseIntegrationTest
{
    public ClubInvitingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_sends_an_invitation_that_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.HikersClub.Id;
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var invitationCountBefore = arrangeContext.Clubs.Single(club => club.Id == clubId).Invitations.Count;

        var response = await client.PostAsync($"/api/social/clubs/{clubId}/invitations/{WellKnownUsers.SecondExplorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Invitations.Should().HaveCount(invitationCountBefore + 1);
        stored.Invitations.Should().Contain(invitation => invitation.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_send_an_invitation()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.HikersClub.Id}/invitations/{WellKnownUsers.SecondExplorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_send_an_invitation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/social/clubs/{ClubSeed.HikersClub.Id}/invitations/{WellKnownUsers.SecondExplorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Invited_user_accepts_and_becomes_a_member_without_a_pending_invitation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var clubId = ClubSeed.SwimmersClubWithSecondExplorersPendingInvitation.Id;

        var response = await client.PutAsync($"/api/social/clubs/{clubId}/invitations/mine/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Members.Should().ContainSingle(member => member.UserId == WellKnownUsers.SecondExplorer)
            .Which.Role.Should().Be(ClubRole.Member);
        stored.Invitations.Should().NotContain(invitation => invitation.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_accept_an_invitation()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.SwimmersClubWithSecondExplorersPendingInvitation.Id}/invitations/mine/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_accept_an_invitation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.SwimmersClubWithSecondExplorersPendingInvitation.Id}/invitations/mine/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_decline_an_invitation()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.SwimmersClubWithSecondExplorersPendingInvitation.Id}/invitations/mine/decline", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_decline_an_invitation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsync(
            $"/api/social/clubs/{ClubSeed.SwimmersClubWithSecondExplorersPendingInvitation.Id}/invitations/mine/decline", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
