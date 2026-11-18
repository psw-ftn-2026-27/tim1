using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallengeManagement;
using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeManagement;

public sealed class LocationChallengeManagementCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/games/location-challenges";

    public LocationChallengeManagementCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_creates_a_location_challenge_that_is_saved_as_a_draft_with_all_its_hidden_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new SaveLocationChallengeDto("Potraga po Limanu", new LocationDto(45.2433, 19.8385), 90,
        [
            new HiddenPointInputDto(new LocationDto(45.2441, 19.8402), ImageSeed.LookoutPhotoNotYetOnAChallenge.Id),
            new HiddenPointInputDto(new LocationDto(45.2425, 19.8371), ImageSeed.UniversityPhotoNotYetOnAChallenge.Id)
        ]);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsJsonAsync(Url, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == createdId);
        stored.Name.Should().Be(request.Name);
        stored.StartLocation.Should().Be(new Location(45.2433, 19.8385));
        stored.ExperiencePoints.Should().Be(request.ExperiencePoints);
        stored.Status.Should().Be(LocationChallengeStatus.Draft);
        Describe(stored.HiddenPoints).Should().BeEquivalentTo(Describe(request.HiddenPoints));
    }

    [Fact]
    public async Task Administrator_changes_a_draft_location_challenge_and_its_hidden_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = LocationChallengeSeed.DraftFortressHunt;
        var request = new SaveLocationChallengeDto("Potraga po Podgrađu", new LocationDto(45.2510, 19.8630), 150,
        [
            new HiddenPointInputDto(new LocationDto(45.2524, 19.8643), ImageSeed.ClockTowerPhoto.Id),
            new HiddenPointInputDto(new LocationDto(45.2508, 19.8662), ImageSeed.LookoutPhotoNotYetOnAChallenge.Id)
        ]);

        var response = await client.PutAsJsonAsync($"{Url}/{seeded.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == seeded.Id);
        stored.Name.Should().Be(request.Name);
        stored.StartLocation.Should().Be(new Location(45.2510, 19.8630));
        stored.ExperiencePoints.Should().Be(request.ExperiencePoints);
        stored.Status.Should().Be(LocationChallengeStatus.Draft);
        Describe(stored.HiddenPoints).Should().BeEquivalentTo(Describe(request.HiddenPoints));
    }

    [Fact]
    public async Task Published_location_challenge_cannot_be_changed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = LocationChallengeSeed.PublishedOldTownHunt;
        var request = new SaveLocationChallengeDto("Potraga po Podgrađu", new LocationDto(45.2510, 19.8630), 150,
            [new HiddenPointInputDto(new LocationDto(45.2508, 19.8662), ImageSeed.LookoutPhotoNotYetOnAChallenge.Id)]);

        var response = await client.PutAsJsonAsync($"{Url}/{seeded.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == seeded.Id);
        stored.Name.Should().Be(seeded.Name);
        stored.StartLocation.Should().Be(seeded.StartLocation);
        stored.ExperiencePoints.Should().Be(seeded.ExperiencePoints);
        stored.Status.Should().Be(LocationChallengeStatus.Published);
        stored.HiddenPoints.Select(point => new { point.Id, point.Location, point.ImageId })
            .Should().BeEquivalentTo(seeded.HiddenPoints.Select(point => new { point.Id, point.Location, point.ImageId }));
    }

    [Fact]
    public async Task Location_challenge_that_does_not_exist_cannot_be_changed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new SaveLocationChallengeDto("Potraga po Podgrađu", new LocationDto(45.2510, 19.8630), 150,
            [new HiddenPointInputDto(new LocationDto(45.2508, 19.8662), ImageSeed.LookoutPhotoNotYetOnAChallenge.Id)]);

        var response = await client.PutAsJsonAsync($"{Url}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Any(challenge => challenge.Name == request.Name).Should().BeFalse();
    }

    [Theory]
    [InlineData("publish", LocationChallengeStatus.Draft, LocationChallengeStatus.Published)]
    [InlineData("retire", LocationChallengeStatus.Published, LocationChallengeStatus.Retired)]
    [InlineData("archive", LocationChallengeStatus.Retired, LocationChallengeStatus.Archived)]
    public async Task Administrator_moves_a_location_challenge_to_the_next_state(
        string command, LocationChallengeStatus startingStatus, LocationChallengeStatus nextStatus)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = GetSeed(startingStatus);

        var response = await client.PostAsync($"{Url}/{seeded.Id}/{command}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Single(challenge => challenge.Id == seeded.Id).Status.Should().Be(nextStatus);
    }

    [Fact]
    public async Task Administrator_deletes_an_archived_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"{Url}/{LocationChallengeSeed.ArchivedQuayHunt.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Any(challenge => challenge.Id == LocationChallengeSeed.ArchivedQuayHunt.Id)
            .Should().BeFalse();
    }

    [Theory]
    [InlineData("create", false, HttpStatusCode.Unauthorized)]
    [InlineData("update", false, HttpStatusCode.Unauthorized)]
    [InlineData("publish", false, HttpStatusCode.Unauthorized)]
    [InlineData("retire", false, HttpStatusCode.Unauthorized)]
    [InlineData("archive", false, HttpStatusCode.Unauthorized)]
    [InlineData("delete", false, HttpStatusCode.Unauthorized)]
    [InlineData("create", true, HttpStatusCode.Forbidden)]
    [InlineData("update", true, HttpStatusCode.Forbidden)]
    [InlineData("publish", true, HttpStatusCode.Forbidden)]
    [InlineData("retire", true, HttpStatusCode.Forbidden)]
    [InlineData("archive", true, HttpStatusCode.Forbidden)]
    [InlineData("delete", true, HttpStatusCode.Forbidden)]
    public async Task Only_an_administrator_can_manage_location_challenges(
        string command, bool signedIn, HttpStatusCode expectedStatus)
    {
        var client = signedIn ? Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer") : Factory.CreateClient();

        var response = await SendCommand(client, command, LocationChallengeSeed.DraftFortressHunt.Id);

        response.StatusCode.Should().Be(expectedStatus);
    }

    private static LocationChallenge GetSeed(LocationChallengeStatus status)
    {
        return status switch
        {
            LocationChallengeStatus.Draft => LocationChallengeSeed.DraftFortressHunt,
            LocationChallengeStatus.Published => LocationChallengeSeed.PublishedOldTownHunt,
            LocationChallengeStatus.Retired => LocationChallengeSeed.RetiredParkHunt,
            LocationChallengeStatus.Archived => LocationChallengeSeed.ArchivedQuayHunt,
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
    }

    private static Task<HttpResponseMessage> SendCommand(HttpClient client, string command, Guid id)
    {
        var request = new SaveLocationChallengeDto("Potraga po Podgrađu", new LocationDto(45.2510, 19.8630), 150,
            [new HiddenPointInputDto(new LocationDto(45.2508, 19.8662), ImageSeed.LookoutPhotoNotYetOnAChallenge.Id)]);

        return command switch
        {
            "create" => client.PostAsJsonAsync(Url, request),
            "update" => client.PutAsJsonAsync($"{Url}/{id}", request),
            "delete" => client.DeleteAsync($"{Url}/{id}"),
            _ => client.PostAsync($"{Url}/{id}/{command}", null)
        };
    }

    private static IEnumerable<(double Latitude, double Longitude, Guid ImageId)> Describe(IEnumerable<HiddenPoint> hiddenPoints)
    {
        return hiddenPoints.Select(point => (point.Location.Latitude, point.Location.Longitude, point.ImageId));
    }

    private static IEnumerable<(double Latitude, double Longitude, Guid ImageId)> Describe(IEnumerable<HiddenPointInputDto> hiddenPoints)
    {
        return hiddenPoints.Select(point => (point.Location.Latitude, point.Location.Longitude, point.ImageId));
    }
}
