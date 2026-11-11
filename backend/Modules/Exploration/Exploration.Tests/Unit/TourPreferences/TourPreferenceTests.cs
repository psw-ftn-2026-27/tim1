using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourPreferences;

public class TourPreferenceTests
{
    private static TourPreference CreatePreference() =>
        new(WellKnownUsers.Explorer, [TourDifficulty.Easy], TransportMode.Walking, ["istorija"], false);

    private static Tour CreatePublishedTour(List<string> tags)
    {
        var tour = new Tour(WellKnownUsers.SecondExplorer, "Šetnja tvrđavom", new string('o', 100), TourDifficulty.Easy, tags);
        tour.AddTransportTime(TransportMode.Walking, 60);
        tour.Publish();
        return tour;
    }

    [Fact]
    public void Preferences_without_difficulty_transport_and_tags_cannot_be_created()
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [], null, [], true);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Preferences_cannot_be_changed_to_have_no_difficulty_transport_and_tags()
    {
        var preference = CreatePreference();

        var change = () => preference.Change([], null, [], true);

        change.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tag_repeated_in_different_letter_case_is_kept_once()
    {
        var preference = new TourPreference(WellKnownUsers.Explorer, [], null, ["Istorija", "istorija", "ISTORIJA"], false);

        preference.Tags.Should().ContainSingle();
    }

    [Fact]
    public void Newly_added_tag_that_is_on_no_published_tour_is_rejected()
    {
        var draftTour = new Tour(WellKnownUsers.SecondExplorer, "Šetnja centrom", "Kratak opis.", TourDifficulty.Easy, ["grad"]);
        var publishedTour = CreatePublishedTour(["istorija"]);

        var check = () => AvailableTags.EnsureSelectable(["grad"], [], [draftTour, publishedTour]);

        check.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tag_is_accepted_when_it_differs_from_a_tour_tag_only_in_letter_case()
    {
        var publishedTour = CreatePublishedTour(["istorija"]);

        var check = () => AvailableTags.EnsureSelectable(["Istorija"], [], [publishedTour]);

        check.Should().NotThrow();
    }

    [Fact]
    public void Already_saved_tag_stays_accepted_after_it_leaves_all_published_tours()
    {
        var publishedTour = CreatePublishedTour(["istorija"]);

        var check = () => AvailableTags.EnsureSelectable(["vino"], ["vino"], [publishedTour]);

        check.Should().NotThrow();
    }
}
