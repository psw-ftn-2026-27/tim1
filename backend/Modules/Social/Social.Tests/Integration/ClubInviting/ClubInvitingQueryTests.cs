using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubInviting;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubInviting;

public class ClubInvitingQueryTests : BaseIntegrationTest
{
    public ClubInvitingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_who_is_not_the_owner_does_not_get_the_sent_invitations_of_another_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(
            $"/api/social/clubs/{ClubSeed.SwimmersClubWithSecondExplorersPendingInvitation.Id}/invitations");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Invited_user_gets_their_pending_invitations_with_the_club_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var club = ClubSeed.SwimmersClubWithSecondExplorersPendingInvitation;

        var response = await client.GetAsync("/api/social/clubs/invitations/mine");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invitations = await response.Content.ReadFromJsonAsync<List<MyInvitationDto>>(JsonOptions);
        invitations.Should().ContainSingle(invitation => invitation.ClubId == club.Id)
            .Which.ClubName.Should().Be(club.Name);
    }
}
