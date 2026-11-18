using FluentAssertions;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public sealed class LocationChallengeTests
{
    [Fact]
    public void New_location_challenge_starts_as_a_draft()
    {
        var challenge = CreateChallenge();

        challenge.Status.Should().Be(LocationChallengeStatus.Draft);
    }

    [Theory]
    [InlineData("", 100, 1)]
    [InlineData("   ", 100, 1)]
    [InlineData("Potraga po tvrđavi", 0, 1)]
    [InlineData("Potraga po tvrđavi", -10, 1)]
    [InlineData("Potraga po tvrđavi", 100, 0)]
    public void Location_challenge_without_a_name_positive_experience_points_or_a_hidden_point_is_not_created(
        string name, int experiencePoints, int hiddenPointCount)
    {
        var hiddenPoints = Enumerable.Range(0, hiddenPointCount).Select(_ => CreateHiddenPoint()).ToArray();

        var creation = () => new LocationChallenge(name, new Location(45.2517, 19.8619), experiencePoints, hiddenPoints);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Hidden_point_without_a_photo_is_not_created()
    {
        var creation = () => new HiddenPoint(new Location(45.2531, 19.8625), Guid.Empty);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_location_challenge_is_changed_with_its_new_hidden_points()
    {
        var challenge = CreateChallenge();
        var newHiddenPoints = new[] { CreateHiddenPoint(), CreateHiddenPoint() };

        challenge.Update("Potraga kroz Stari grad", new Location(45.2551, 19.8451), 80, newHiddenPoints);

        challenge.Name.Should().Be("Potraga kroz Stari grad");
        challenge.StartLocation.Should().Be(new Location(45.2551, 19.8451));
        challenge.ExperiencePoints.Should().Be(80);
        challenge.HiddenPoints.Should().Equal(newHiddenPoints);
    }

    [Theory]
    [InlineData(LocationChallengeStatus.Published)]
    [InlineData(LocationChallengeStatus.Retired)]
    [InlineData(LocationChallengeStatus.Archived)]
    public void Only_a_draft_location_challenge_is_changed(LocationChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);
        var originalHiddenPoints = challenge.HiddenPoints.ToArray();

        var update = () => challenge.Update("Potraga kroz Stari grad", new Location(45.2551, 19.8451), 80, [CreateHiddenPoint()]);

        update.Should().Throw<DomainException>();
        challenge.Name.Should().Be("Potraga po tvrđavi");
        challenge.StartLocation.Should().Be(new Location(45.2517, 19.8619));
        challenge.ExperiencePoints.Should().Be(120);
        challenge.HiddenPoints.Should().Equal(originalHiddenPoints);
    }

    private static LocationChallenge CreateChallenge() =>
        new("Potraga po tvrđavi", new Location(45.2517, 19.8619), 120, [CreateHiddenPoint()]);

    private static HiddenPoint CreateHiddenPoint() => new(new Location(45.2531, 19.8625), Guid.NewGuid());

    // Walks a new challenge forward through the lifecycle, which follows the order of the
    // enum, until it reaches the given status.
    private static LocationChallenge CreateChallengeIn(LocationChallengeStatus status)
    {
        var challenge = CreateChallenge();
        if (status >= LocationChallengeStatus.Published)
        {
            challenge.Publish();
        }
        if (status >= LocationChallengeStatus.Retired)
        {
            challenge.Retire();
        }
        if (status >= LocationChallengeStatus.Archived)
        {
            challenge.Archive();
        }
        return challenge;
    }
}
