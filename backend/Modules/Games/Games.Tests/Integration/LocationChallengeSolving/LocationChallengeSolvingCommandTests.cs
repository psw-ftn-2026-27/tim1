using System.Net;
using FluentAssertions;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeSolving;

public sealed class LocationChallengeSolvingCommandTests : BaseIntegrationTest
{
    private static readonly string ActivateUrl =
        $"/api/games/location-challenges/{LocationChallengeSeed.PublishedOldTownHunt.Id}/activate";

    public LocationChallengeSolvingCommandTests(GamesApiFactory factory) : base(factory) { }

    // Games' tests never seed Exploration, so no explorer has a saved position here.
    [Fact]
    public async Task Explorer_without_a_saved_position_cannot_activate_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.LocationChallengeAttempts.Count();

        var response = await client.PostAsync(ActivateUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeAttempts.Count().Should().Be(attemptCountBefore);
    }

    [Fact]
    public async Task Anonymous_visitor_cannot_activate_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(ActivateUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(ActivateUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
