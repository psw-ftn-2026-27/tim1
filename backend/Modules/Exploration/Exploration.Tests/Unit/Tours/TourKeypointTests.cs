using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourKeypointTests
{
    private static readonly string LongDescription = new('o', 100);
    private static readonly Coordinate Start = new(45.0, 19.0);
    private static readonly Coordinate Middle = new(45.1, 19.1);
    private static readonly Coordinate End = new(45.2, 19.0);

    private static Tour CreateDraftTour() =>
        new(WellKnownUsers.Explorer, "Šetnja tvrđavom", LongDescription, TourDifficulty.Easy, ["istorija"], new GroupSize(1, 10));

    private static Tour CreatePublishedTour()
    {
        var tour = CreateDraftTour();
        tour.AddTransportTime(TransportMode.Car, 30);
        tour.AddKeypoint("Prva tačka", "Opis prve tačke.", Guid.NewGuid(), null, false, Start);
        tour.AddKeypoint("Druga tačka", "Opis druge tačke.", Guid.NewGuid(), null, false, End);
        tour.Publish(0);
        return tour;
    }

    [Fact]
    public void Keypoint_cannot_be_added_to_a_tour_that_is_not_a_draft()
    {
        var tour = CreatePublishedTour();

        var addition = () => tour.AddKeypoint("Nova tačka", "Opis nove tačke.", Guid.NewGuid(), null, false, Middle);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Keypoint_cannot_be_updated_in_a_tour_that_is_not_a_draft()
    {
        var tour = CreatePublishedTour();

        var update = () => tour.UpdateKeypoint(tour.Keypoints[0].Id, "Nov naziv", "Opis.", Guid.NewGuid(), null, Middle);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Keypoint_cannot_be_removed_from_a_tour_that_is_not_a_draft()
    {
        var tour = CreatePublishedTour();

        var removal = () => tour.RemoveKeypoint(tour.Keypoints[0].Id);

        removal.Should().Throw<DomainException>();
    }

    [Fact]
    public void Keypoint_visibility_can_be_changed_in_a_published_tour()
    {
        var tour = CreatePublishedTour();

        tour.SetKeypointVisibility(tour.Keypoints[0].Id, true);

        tour.Keypoints[0].PubliclyVisible.Should().BeTrue();
    }

    [Theory]
    [InlineData(-90.1, 0)]
    [InlineData(90.1, 0)]
    [InlineData(0, -180.1)]
    [InlineData(0, 180.1)]
    public void Coordinates_outside_the_allowed_range_are_rejected(double latitude, double longitude)
    {
        var creation = () => new Coordinate(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Opis.", true)]
    [InlineData("Naziv", " ", true)]
    [InlineData("Naziv", "Opis.", false)]
    public void Keypoint_without_a_name_a_description_or_an_image_is_rejected(string name, string description, bool hasImage)
    {
        var tour = CreateDraftTour();
        var imageId = hasImage ? Guid.NewGuid() : Guid.Empty;

        var addition = () => tour.AddKeypoint(name, description, imageId, null, false, Start);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_length_is_set_in_kilometers()
    {
        var tour = CreateDraftTour();

        tour.SetLength(12.5);

        tour.Length.Should().Be(12.5);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Tour_length_that_is_not_a_non_negative_number_is_rejected(double lengthKm)
    {
        var tour = CreateDraftTour();

        var change = () => tour.SetLength(lengthKm);

        change.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_length_cannot_be_set_in_a_tour_that_is_not_a_draft()
    {
        var tour = CreatePublishedTour();

        var change = () => tour.SetLength(5);

        change.Should().Throw<DomainException>();
    }
}
