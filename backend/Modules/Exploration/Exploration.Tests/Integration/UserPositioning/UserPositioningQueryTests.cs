using System.Net;
using System.Net.Http.Json;
using Exploration.Application.UserPositioning;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.UserPositioning;

public class UserPositioningQueryTests : BaseIntegrationTest
{
    private const string PositionUrl = "/api/exploration/user-positions/mine";

    public UserPositioningQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_gets_their_saved_coordinates_and_update_time()
    {
        var saved = UserPositionSeed.ExplorersPosition;
        var client = Factory.CreateClientFor(saved.UserId, "explorer");

        var response = await client.GetFromJsonAsync<CurrentPositionDto>(PositionUrl, JsonOptions);

        response!.Latitude.Should().Be(saved.Latitude);
        response.Longitude.Should().Be(saved.Longitude);
        response.UpdatedAt.Should().BeCloseTo(saved.UpdatedAt, TimeSpan.FromMilliseconds(1));
        response.UpdatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Explorer_without_a_saved_position_gets_no_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(PositionUrl);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
