using System.Net;
using System.Net.Http.Json;
using Exploration.Application.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourAuthoring;

public class TourAuthoringQueryTests : BaseIntegrationTest
{
    public TourAuthoringQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_lists_only_their_own_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<List<TourDto>>("/api/exploration/tours/mine", JsonOptions);

        tours.Should().HaveCount(TourSeed.All.Count(tour => tour.AuthorId == WellKnownUsers.Explorer));
        tours.Should().OnlyContain(tour => tour.AuthorId == WellKnownUsers.Explorer);
        tours.Should().NotContain(tour => tour.Id == TourSeed.SecondExplorersTrail.Id);
    }

    [Fact]
    public async Task Author_gets_their_tour_with_its_length()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = TourSeed.DraftWithKeypoints;

        var tour = await client.GetFromJsonAsync<TourDto>($"/api/exploration/tours/mine/{seeded.Id}", JsonOptions);

        tour!.Name.Should().Be(seeded.Name);
        tour.Length.Should().BeApproximately(seeded.Length, 0.0001);
    }

    [Fact]
    public async Task Explorer_cannot_get_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/mine/{TourSeed.DraftWithKeypoints.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
