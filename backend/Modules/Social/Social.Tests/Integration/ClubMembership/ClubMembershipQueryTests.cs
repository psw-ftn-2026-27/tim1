using System.Net;
using FluentAssertions;
using Shared.Tests;
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
}
