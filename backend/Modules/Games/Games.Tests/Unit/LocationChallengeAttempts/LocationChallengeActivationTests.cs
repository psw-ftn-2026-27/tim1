using FluentAssertions;
using Games.Domain.LocationChallengeAttempts;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallengeAttempts;

public sealed class LocationChallengeActivationTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Location Start = new(45.2551, 19.8451);
    private static readonly Location NearHiddenPoint = new(45.2553, 19.8451);
    private static readonly Location FarthestHiddenPoint = new(45.2551, 19.8459);

    [Theory]
    [InlineData(LocationChallengeStatus.Draft)]
    [InlineData(LocationChallengeStatus.Retired)]
    [InlineData(LocationChallengeStatus.Archived)]
    public void Location_challenge_that_is_not_published_cannot_be_activated(LocationChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var activation = () => new LocationChallengeActivation().Activate(challenge, null, UserId, Start, 30);

        activation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.0004)]
    public void Explorer_within_fifty_meters_of_the_start_activates_the_challenge(double latitudeOffset)
    {
        var challenge = CreateChallengeIn(LocationChallengeStatus.Published);
        var position = new Location(Start.Latitude + latitudeOffset, Start.Longitude);

        var attempt = new LocationChallengeActivation().Activate(challenge, null, UserId, position, 30);

        attempt.ChallengeId.Should().Be(challenge.Id);
        attempt.UserId.Should().Be(UserId);
    }

    [Theory]
    [InlineData(0.0005)]
    [InlineData(0.01)]
    public void Explorer_farther_than_fifty_meters_from_the_start_cannot_activate_the_challenge(double latitudeOffset)
    {
        var challenge = CreateChallengeIn(LocationChallengeStatus.Published);
        var position = new Location(Start.Latitude + latitudeOffset, Start.Longitude);

        var activation = () => new LocationChallengeActivation().Activate(challenge, null, UserId, position, 30);

        activation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(37.5)]
    [InlineData(100)]
    public void Search_circle_is_centered_on_the_start_and_reaches_the_farthest_hidden_point_plus_the_extra(double extraMeters)
    {
        var challenge = CreateChallengeIn(LocationChallengeStatus.Published);

        var attempt = new LocationChallengeActivation().Activate(challenge, null, UserId, Start, extraMeters);

        attempt.Circle.Center.Should().Be(Start);
        attempt.Circle.DiameterMeters.Should().BeApproximately(Start.DistanceTo(FarthestHiddenPoint) + extraMeters, 0.001);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(100.1)]
    public void Search_circle_extra_outside_zero_to_one_hundred_meters_is_rejected(double extraMeters)
    {
        var challenge = CreateChallengeIn(LocationChallengeStatus.Published);

        var activation = () => new LocationChallengeActivation().Activate(challenge, null, UserId, Start, extraMeters);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Repeated_activation_continues_the_started_attempt_with_the_same_circle()
    {
        var challenge = CreateChallengeIn(LocationChallengeStatus.Published);
        var started = new LocationChallengeActivation().Activate(challenge, null, UserId, Start, 30);

        var attempt = new LocationChallengeActivation().Activate(challenge, started, UserId, Start, 90);

        attempt.Should().BeSameAs(started);
        attempt.Circle.Should().Be(started.Circle);
    }

    [Fact]
    public void Attempt_started_before_the_challenge_was_retired_can_be_continued()
    {
        var challenge = CreateChallengeIn(LocationChallengeStatus.Published);
        var started = new LocationChallengeActivation().Activate(challenge, null, UserId, Start, 30);
        challenge.Retire();

        var attempt = new LocationChallengeActivation().Activate(challenge, started, UserId, Start, 90);

        attempt.Should().BeSameAs(started);
        attempt.Circle.Should().Be(started.Circle);
    }

    // Walks a new challenge forward through the lifecycle, which follows the order of the
    // enum, until it reaches the given status.
    private static LocationChallenge CreateChallengeIn(LocationChallengeStatus status)
    {
        var challenge = new LocationChallenge("Potraga kroz Stari grad", Start, 80,
        [
            new HiddenPoint(NearHiddenPoint, Guid.NewGuid()),
            new HiddenPoint(FarthestHiddenPoint, Guid.NewGuid())
        ]);
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
