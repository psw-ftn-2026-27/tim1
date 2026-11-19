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

    private static readonly string CheckUrl =
        $"/api/games/location-challenges/{LocationChallengeSeed.PublishedSquareHunt.Id}/attempt/check";

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

    // A successful check needs a saved position, which Games' tests cannot seed, so the
    // discovery rules are covered by the unit tests of LocationDiscovery.
    [Fact]
    public async Task Explorer_without_a_saved_position_cannot_check_a_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = LocationChallengeAttemptSeed.ExplorersSquareAttemptWithOnePointDiscovered;

        var response = await client.PostAsync(CheckUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var attempt = assertContext.LocationChallengeAttempts.Single(attempt => attempt.Id == seeded.Id);
        attempt.DiscoveredPointIds.Should().BeEquivalentTo(seeded.DiscoveredPointIds);
        attempt.SolvedAt.Should().BeNull();
    }

    [Fact]
    public async Task Explorer_cannot_check_a_location_in_a_challenge_they_have_not_activated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var challenge = LocationChallengeSeed.PublishedOldTownHunt;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var completionCountBefore = arrangeContext.ChallengeCompletions.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{challenge.Id}/attempt/check", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeAttempts
            .Any(attempt => attempt.ChallengeId == challenge.Id && attempt.UserId == WellKnownUsers.SecondExplorer)
            .Should().BeFalse();
        assertContext.ChallengeCompletions.Count().Should().Be(completionCountBefore);
    }

    [Fact]
    public async Task Anonymous_visitor_cannot_check_a_location()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(CheckUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_check_a_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(CheckUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
