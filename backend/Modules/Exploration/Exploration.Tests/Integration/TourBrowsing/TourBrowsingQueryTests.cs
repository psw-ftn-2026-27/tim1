using System.Net.Http.Json;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourBrowsing;

public class TourBrowsingQueryTests : BaseIntegrationTest
{
    public TourBrowsingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Only_published_tours_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().OnlyContain(tour => tour.Status == TourStatus.Published);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.FortressWalk.Id);
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Published_tours_are_listed_page_by_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Tours_are_found_by_part_of_the_title_regardless_of_letter_case()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?title=vINOGRADI", JsonOptions);

        tours!.Items.Should().ContainSingle(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().OnlyContain(tour => tour.Name.Contains("vinogradi", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Tours_are_listed_when_they_have_at_least_one_of_the_chosen_difficulties()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficulties=Moderate&difficulties=Hard", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedLakeside.Id);
        tours.Items.Should().OnlyContain(tour => tour.Difficulty == TourDifficulty.Moderate || tour.Difficulty == TourDifficulty.Hard);
    }

    [Fact]
    public async Task Tours_are_listed_when_they_have_a_time_for_at_least_one_of_the_chosen_transports()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?transports=Bicycle&transports=Car", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedLakeside.Id);
        tours.Items.Should().OnlyContain(tour => tour.TransportTimes.Any(time => time.Transport == TransportMode.Bicycle || time.Transport == TransportMode.Car));
    }

    [Fact]
    public async Task Tours_are_listed_only_when_they_have_all_the_added_tags()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?tags=priroda&tags=vino", JsonOptions);

        tours!.Items.Should().ContainSingle(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().OnlyContain(tour => tour.Tags.Contains("priroda") && tour.Tags.Contains("vino"));
    }

    [Fact]
    public async Task Tours_matching_every_kind_of_criterion_at_once_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?title=fruš&difficulties=Hard&transports=Car&tags=istorija", JsonOptions);

        tours!.Items.Should().ContainSingle(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Tours_missing_one_kind_of_criterion_are_not_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?title=fruš&difficulties=Easy&transports=Car&tags=istorija", JsonOptions);

        tours!.Items.Should().BeEmpty();
        tours.TotalCount.Should().Be(0);
    }

    [Theory]
    [InlineData("DifficultyAscending", false)]
    [InlineData("DifficultyDescending", true)]
    public async Task Tours_are_sorted_by_difficulty_from_easy_to_hard_and_the_direction_reverses_the_order(string sort, bool reversed)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>($"/api/exploration/tours/published?sort={sort}", JsonOptions);

        var published = TourSeed.All.Where(tour => tour.Status == TourStatus.Published).Select(tour => tour.Difficulty);
        var expected = reversed ? published.OrderByDescending(difficulty => difficulty) : published.OrderBy(difficulty => difficulty);
        tours!.Items.Select(tour => tour.Difficulty).Should().Equal(expected);
    }

    [Fact]
    public async Task Without_criteria_and_sorting_all_published_tours_are_listed_from_the_most_recently_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);

        var expectedIds = TourSeed.All
            .Where(tour => tour.Status == TourStatus.Published)
            .OrderByDescending(tour => tour.PublishedAt)
            .Select(tour => tour.Id);
        tours!.Items.Select(tour => tour.Id).Should().Equal(expectedIds);
    }
}
