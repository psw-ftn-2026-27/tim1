using FluentAssertions;
using Games.Domain.LocationChallengeAttempts;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallengeAttempts;

public sealed class LocationDiscoveryTests
{
    private static readonly Location Start = new(45.2551, 19.8451);
    private static readonly Location Cathedral = new(45.2553, 19.8451);
    private static readonly Location NextToCathedral = new(45.25532, 19.8451);
    private static readonly Location Fountain = new(45.2551, 19.8459);

    [Theory]
    [InlineData(0)]
    [InlineData(0.00004)]
    public void Explorer_within_five_meters_of_a_hidden_point_discovers_it(double latitudeOffset)
    {
        var (challenge, attempt) = ActivateChallengeWith(Cathedral, Fountain);
        var position = new Location(Cathedral.Latitude - latitudeOffset, Cathedral.Longitude);

        new LocationDiscovery().Check(challenge, attempt, position);

        attempt.DiscoveredPointIds.Should().Equal(challenge.HiddenPoints[0].Id);
    }

    [Theory]
    [InlineData(0.00005)]
    [InlineData(0.001)]
    public void Explorer_farther_than_five_meters_from_every_hidden_point_discovers_nothing(double latitudeOffset)
    {
        var (challenge, attempt) = ActivateChallengeWith(Cathedral, Fountain);
        var position = new Location(Cathedral.Latitude - latitudeOffset, Cathedral.Longitude);

        var check = () => new LocationDiscovery().Check(challenge, attempt, position);

        check.Should().Throw<DomainException>();
        attempt.DiscoveredPointIds.Should().BeEmpty();
    }

    [Fact]
    public void Explorer_at_an_already_discovered_hidden_point_discovers_nothing()
    {
        var (challenge, attempt) = ActivateChallengeWith(Cathedral, Fountain);
        new LocationDiscovery().Check(challenge, attempt, Cathedral);

        var check = () => new LocationDiscovery().Check(challenge, attempt, Cathedral);

        check.Should().Throw<DomainException>();
    }

    [Fact]
    public void One_check_discovers_every_undiscovered_hidden_point_within_five_meters()
    {
        var (challenge, attempt) = ActivateChallengeWith(Cathedral, NextToCathedral, Fountain);
        var between = new Location(45.25531, 19.8451);

        new LocationDiscovery().Check(challenge, attempt, between);

        attempt.DiscoveredPointIds.Should().BeEquivalentTo([challenge.HiddenPoints[0].Id, challenge.HiddenPoints[1].Id]);
    }

    [Fact]
    public void Attempt_with_hidden_points_left_to_discover_is_not_solved()
    {
        var (challenge, attempt) = ActivateChallengeWith(Cathedral, Fountain);

        new LocationDiscovery().Check(challenge, attempt, Fountain);

        attempt.SolvedAt.Should().BeNull();
        attempt.AwardedExperiencePoints.Should().BeNull();
    }

    [Fact]
    public void Discovering_the_last_hidden_point_solves_the_attempt_and_awards_the_challenge_experience_points()
    {
        var (challenge, attempt) = ActivateChallengeWith(Cathedral, Fountain);
        new LocationDiscovery().Check(challenge, attempt, Fountain);

        new LocationDiscovery().Check(challenge, attempt, Cathedral);

        attempt.SolvedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        attempt.AwardedExperiencePoints.Should().Be(challenge.ExperiencePoints);
    }

    [Fact]
    public void Solved_attempt_cannot_discover_again_so_its_experience_points_are_awarded_once()
    {
        var (challenge, attempt) = ActivateChallengeWith(Cathedral);
        new LocationDiscovery().Check(challenge, attempt, Cathedral);

        var discovery = () => attempt.Discover(challenge, [challenge.HiddenPoints[0].Id]);

        discovery.Should().Throw<DomainException>();
    }

    private static (LocationChallenge Challenge, LocationChallengeAttempt Attempt) ActivateChallengeWith(params Location[] hiddenPointLocations)
    {
        var challenge = new LocationChallenge("Potraga kroz Stari grad", Start, 80,
            hiddenPointLocations.Select(location => new HiddenPoint(location, Guid.NewGuid())).ToList());
        challenge.Publish();
        var attempt = new LocationChallengeActivation().Activate(challenge, null, Guid.NewGuid(), Start, 30);
        return (challenge, attempt);
    }
}
