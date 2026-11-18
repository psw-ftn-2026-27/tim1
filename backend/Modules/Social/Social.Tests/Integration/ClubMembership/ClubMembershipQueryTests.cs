using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubMembership;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubMembership;

public class ClubMembershipQueryTests : BaseIntegrationTest
{
    public ClubMembershipQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_who_is_neither_owner_nor_moderator_does_not_get_the_pending_requests_of_another_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(
            $"/api/social/clubs/{ClubSeed.RunnersClubWithSecondExplorersPendingRequest.Id}/membership-requests");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Moderator_gets_the_pending_requests_of_their_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var club = ClubSeed.ReadersClubWithSecondExplorerAsModeratorAndThirdExplorersPendingRequest;

        var requests = await client.GetFromJsonAsync<List<MembershipRequestDto>>(
            $"/api/social/clubs/{club.Id}/membership-requests", JsonOptions);

        requests!.Select(request => request.UserId)
            .Should().BeEquivalentTo(club.MembershipRequests.Select(request => request.UserId));
    }
}
