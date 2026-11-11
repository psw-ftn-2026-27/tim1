using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPreferenceEditing;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferenceEditing;

public class TourPreferenceEditingCommandTests : BaseIntegrationTest
{
    public TourPreferenceEditingCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task First_save_stores_the_tourists_preferences()
    {
        var userId = Guid.NewGuid();
        var client = Factory.CreateClientFor(userId, "explorer");
        var request = new TourPreferenceDto([TourDifficulty.Hard], TransportMode.Bicycle, ["vino"], true);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore + 1);
        var stored = assertContext.TourPreferences.Single(preference => preference.UserId == userId);
        stored.Difficulties.Should().Equal(TourDifficulty.Hard);
        stored.Transport.Should().Be(TransportMode.Bicycle);
        stored.Tags.Should().Equal("vino");
        stored.OpensSearch.Should().BeTrue();
    }

    [Fact]
    public async Task Saving_again_changes_the_existing_preferences_without_creating_a_second_set()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TourPreferenceDto([TourDifficulty.Hard], null, ["vino"], false);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
        var stored = assertContext.TourPreferences.Single(preference => preference.UserId == WellKnownUsers.Explorer);
        stored.Id.Should().Be(TourPreferenceSeed.ExplorersPreference.Id);
        stored.Difficulties.Should().Equal(TourDifficulty.Hard);
        stored.Transport.Should().BeNull();
        stored.Tags.Should().Equal("vino");
        stored.OpensSearch.Should().BeFalse();
    }

    [Fact]
    public async Task Tourist_saving_their_preferences_does_not_change_anyone_elses()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TourPreferenceDto([TourDifficulty.Easy], TransportMode.Car, ["priroda"], false);

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var others = assertContext.TourPreferences.Single(preference => preference.Id == TourPreferenceSeed.SecondExplorersPreference.Id);
        others.Difficulties.Should().Equal(TourPreferenceSeed.SecondExplorersPreference.Difficulties);
        others.Transport.Should().Be(TourPreferenceSeed.SecondExplorersPreference.Transport);
        others.Tags.Should().Equal(TourPreferenceSeed.SecondExplorersPreference.Tags);
        others.OpensSearch.Should().Be(TourPreferenceSeed.SecondExplorersPreference.OpensSearch);
    }

    [Fact]
    public async Task Saving_with_a_tag_that_is_not_offered_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TourPreferenceDto([TourDifficulty.Hard], null, ["grad"], false);

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourPreferences.Single(preference => preference.UserId == WellKnownUsers.Explorer);
        stored.Difficulties.Should().Equal(TourPreferenceSeed.ExplorersPreference.Difficulties);
        stored.Tags.Should().Equal(TourPreferenceSeed.ExplorersPreference.Tags);
    }

    [Fact]
    public async Task Anonymous_user_cannot_save_preferences()
    {
        var client = Factory.CreateClient();
        var request = new TourPreferenceDto([TourDifficulty.Easy], null, [], false);

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Deleting_removes_the_preferences_together_with_the_search_flag()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Any(preference => preference.Id == TourPreferenceSeed.ExplorersPreference.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_preferences()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
