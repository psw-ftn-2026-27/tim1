using System.Net;
using System.Net.Http.Json;
using Exploration.Application.KeypointAuthoring;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeypointAuthoring;

public class KeypointAuthoringQueryTests : BaseIntegrationTest
{
    public KeypointAuthoringQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_gets_the_keypoints_of_their_tour_in_the_order_they_were_added()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = TourSeed.DraftWithKeypoints;

        var keypoints = await client.GetFromJsonAsync<List<KeypointDto>>($"/api/exploration/tours/{seeded.Id}/keypoints", JsonOptions);

        keypoints!.Select(keypoint => keypoint.Id).Should().Equal(seeded.Keypoints.Select(keypoint => keypoint.Id));
    }

    [Fact]
    public async Task Explorer_cannot_get_the_keypoints_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.DraftWithKeypoints.Id}/keypoints");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Author_gets_the_image_of_their_keypoint()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync(KeypointImageUrl());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/webp");
        (await response.Content.ReadAsByteArrayAsync()).Should().Equal(ExplorationApiFactory.SeededImageContent);
    }

    [Fact]
    public async Task Explorer_cannot_get_the_image_of_another_authors_keypoint()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(KeypointImageUrl());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static string KeypointImageUrl() =>
        $"/api/exploration/tours/{TourSeed.DraftWithKeypoints.Id}/keypoints/{TourSeed.DraftWithKeypoints.Keypoints[0].Id}/image";
}
