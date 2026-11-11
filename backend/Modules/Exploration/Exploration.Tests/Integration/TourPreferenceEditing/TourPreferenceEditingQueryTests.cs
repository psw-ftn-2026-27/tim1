using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPreferenceEditing;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferenceEditing;

public class TourPreferenceEditingQueryTests : BaseIntegrationTest
{
    public TourPreferenceEditingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_gets_only_their_own_preferences()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var preference = await client.GetFromJsonAsync<TourPreferenceDto>("/api/exploration/tour-preferences/mine", JsonOptions);

        var own = TourPreferenceSeed.ExplorersPreference;
        preference!.Difficulties.Should().Equal(own.Difficulties);
        preference.Transport.Should().Be(own.Transport);
        preference.Tags.Should().Equal(own.Tags);
        preference.OpensSearch.Should().Be(own.OpensSearch);
    }

    [Fact]
    public async Task Tourist_without_preferences_gets_nothing()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.GetAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Offered_tags_come_only_from_published_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tags = await client.GetFromJsonAsync<List<string>>("/api/exploration/tour-preferences/available-tags", JsonOptions);

        tags.Should().BeEquivalentTo(TourSeed.All
            .Where(tour => tour.Status == TourStatus.Published)
            .SelectMany(tour => tour.Tags)
            .Distinct());
    }
}
