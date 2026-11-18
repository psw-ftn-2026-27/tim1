using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Games.Application.LocationChallenges;
using Games.Application.LocationChallengeSolving;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeSolving;

public sealed class LocationChallengeSolvingQueryTests : BaseIntegrationTest
{
    private const string Url = "/api/games/location-challenges";

    public LocationChallengeSolvingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Attempt_shows_the_search_circle_and_the_photos_but_not_the_hidden_point_locations()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = LocationChallengeSeed.PublishedOldTownHunt;
        var seeded = LocationChallengeAttemptSeed.ExplorersOldTownAttempt;

        var response = await client.GetAsync($"{Url}/{challenge.Id}/attempt");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var attempt = body.Deserialize<LocationChallengeAttemptDto>(JsonOptions);
        attempt!.Id.Should().Be(seeded.Id);
        attempt.Circle.Should().Be(new SearchCircleDto(
            new LocationDto(seeded.Circle.Center.Latitude, seeded.Circle.Center.Longitude),
            seeded.Circle.DiameterMeters));
        attempt.Photos.Select(photo => photo.ImageId)
            .Should().BeEquivalentTo(challenge.HiddenPoints.Select(point => point.ImageId));
        body.EnumerateObject().Select(property => property.Name).Should().BeEquivalentTo(
            "id", "challengeId", "challengeName", "circle", "photos", "remainingPointCount", "isSolved",
            "startedAt", "solvedAt", "awardedExperiencePoints");
        body.GetProperty("photos").EnumerateArray()
            .SelectMany(photo => photo.EnumerateObject().Select(property => property.Name))
            .Should().OnlyContain(name => name == "imageId" || name == "isDiscovered");
    }

    [Fact]
    public async Task Attempt_shows_which_photos_are_discovered_and_how_many_points_remain()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = LocationChallengeSeed.PublishedSquareHunt;
        var seeded = LocationChallengeAttemptSeed.ExplorersSquareAttemptWithOnePointDiscovered;

        var response = await client.GetAsync($"{Url}/{challenge.Id}/attempt");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var attempt = await response.Content.ReadFromJsonAsync<LocationChallengeAttemptDto>(JsonOptions);
        attempt!.Photos.Should().BeEquivalentTo(challenge.HiddenPoints.Select(point =>
            new AttemptPhotoDto(point.ImageId, seeded.DiscoveredPointIds.Contains(point.Id))));
        attempt.RemainingPointCount.Should().Be(challenge.HiddenPoints.Count - seeded.DiscoveredPointIds.Count);
        attempt.IsSolved.Should().BeFalse();
        attempt.SolvedAt.Should().BeNull();
        attempt.AwardedExperiencePoints.Should().BeNull();
    }

    [Fact]
    public async Task Solved_attempt_shows_every_photo_discovered_and_the_awarded_experience_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var challenge = LocationChallengeSeed.PublishedSquareHunt;
        var seeded = LocationChallengeAttemptSeed.SecondExplorersSolvedSquareAttempt;

        var response = await client.GetAsync($"{Url}/{challenge.Id}/attempt");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var attempt = await response.Content.ReadFromJsonAsync<LocationChallengeAttemptDto>(JsonOptions);
        attempt!.Photos.Should().OnlyContain(photo => photo.IsDiscovered);
        attempt.RemainingPointCount.Should().Be(0);
        attempt.IsSolved.Should().BeTrue();
        attempt.SolvedAt.Should().BeCloseTo(seeded.SolvedAt!.Value, TimeSpan.FromMilliseconds(1));
        attempt.AwardedExperiencePoints.Should().Be(challenge.ExperiencePoints);
    }

    [Fact]
    public async Task Explorer_who_activated_a_challenge_gets_its_photo()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = LocationChallengeSeed.PublishedOldTownHunt;

        var response = await client.GetAsync($"{Url}/{challenge.Id}/attempt/images/{ImageSeed.CathedralPhoto.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().Equal(GamesApiFactory.SeededImageContent);
    }

    [Fact]
    public async Task Explorer_who_has_not_activated_a_challenge_does_not_get_its_photo()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var challenge = LocationChallengeSeed.PublishedOldTownHunt;

        var response = await client.GetAsync($"{Url}/{challenge.Id}/attempt/images/{ImageSeed.CathedralPhoto.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Explorer_does_not_get_a_photo_of_another_challenge_through_an_attempt()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = LocationChallengeSeed.PublishedOldTownHunt;

        var response = await client.GetAsync($"{Url}/{challenge.Id}/attempt/images/{ImageSeed.FortressGatePhoto.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Explorer_without_a_saved_position_gets_no_position_and_no_nearby_challenges()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"{Url}/nearby");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var nearby = await response.Content.ReadFromJsonAsync<NearbyLocationChallengesDto>(JsonOptions);
        nearby!.Position.Should().BeNull();
        nearby.Challenges.Should().BeEmpty();
    }
}
