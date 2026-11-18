using System.Net;
using System.Net.Http.Json;
using Exploration.Application.UserPositioning;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.UserPositioning;

public class UserPositioningCommandTests : BaseIntegrationTest
{
    private const string PositionUrl = "/api/exploration/user-positions/mine";

    public UserPositioningCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task First_save_records_the_signed_in_explorers_position()
    {
        var userId = WellKnownUsers.SecondExplorer;
        var client = Factory.CreateClientFor(userId, "explorer");
        var request = new SaveCurrentPositionDto(44.8, 20.46);
        var beforeSave = DateTime.UtcNow;

        var response = await client.PutAsJsonAsync(PositionUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<ExplorationDbContext>();
        var stored = context.UserPositions.Single(position => position.UserId == userId);
        stored.Latitude.Should().Be(44.8);
        stored.Longitude.Should().Be(20.46);
        stored.UpdatedAt.Should().BeOnOrAfter(beforeSave);
    }

    [Fact]
    public async Task Saving_again_updates_the_same_position()
    {
        var previous = UserPositionSeed.ExplorersPosition;
        var client = Factory.CreateClientFor(previous.UserId, "explorer");
        var request = new SaveCurrentPositionDto(43.32, 21.9);

        var response = await client.PutAsJsonAsync(PositionUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<ExplorationDbContext>();
        var stored = context.UserPositions.Single(position => position.UserId == previous.UserId);
        stored.Id.Should().Be(previous.Id);
        stored.Latitude.Should().Be(43.32);
        stored.Longitude.Should().Be(21.9);
        stored.UpdatedAt.Should().BeAfter(previous.UpdatedAt);
    }

    [Fact]
    public async Task Anonymous_visitor_cannot_save_a_position()
    {
        var client = Factory.CreateClient();
        var request = new SaveCurrentPositionDto(45.25, 19.83);

        var response = await client.PutAsJsonAsync(PositionUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_save_an_explorers_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new SaveCurrentPositionDto(45.25, 19.83);

        var response = await client.PutAsJsonAsync(PositionUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
