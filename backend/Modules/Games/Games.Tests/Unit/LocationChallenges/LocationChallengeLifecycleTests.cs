using FluentAssertions;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public sealed class LocationChallengeLifecycleTests
{
    [Theory]
    [InlineData(LocationChallengeStatus.Draft, LocationChallengeStatus.Published)]
    [InlineData(LocationChallengeStatus.Published, LocationChallengeStatus.Retired)]
    [InlineData(LocationChallengeStatus.Retired, LocationChallengeStatus.Archived)]
    public void Location_challenge_advances_from_the_previous_state(
        LocationChallengeStatus startingStatus, LocationChallengeStatus nextStatus)
    {
        var challenge = CreateChallengeIn(startingStatus);
        var transition = GetTransition(challenge, nextStatus);

        transition();

        challenge.Status.Should().Be(nextStatus);
    }

    [Theory]
    [InlineData(LocationChallengeStatus.Published, LocationChallengeStatus.Published)]
    [InlineData(LocationChallengeStatus.Retired, LocationChallengeStatus.Published)]
    [InlineData(LocationChallengeStatus.Archived, LocationChallengeStatus.Published)]
    [InlineData(LocationChallengeStatus.Draft, LocationChallengeStatus.Retired)]
    [InlineData(LocationChallengeStatus.Retired, LocationChallengeStatus.Retired)]
    [InlineData(LocationChallengeStatus.Archived, LocationChallengeStatus.Retired)]
    [InlineData(LocationChallengeStatus.Draft, LocationChallengeStatus.Archived)]
    [InlineData(LocationChallengeStatus.Published, LocationChallengeStatus.Archived)]
    [InlineData(LocationChallengeStatus.Archived, LocationChallengeStatus.Archived)]
    public void Location_challenge_does_not_advance_from_any_other_state(
        LocationChallengeStatus startingStatus, LocationChallengeStatus requestedStatus)
    {
        var challenge = CreateChallengeIn(startingStatus);
        var transition = GetTransition(challenge, requestedStatus);

        transition.Should().Throw<DomainException>();

        challenge.Status.Should().Be(startingStatus);
    }

    [Theory]
    [InlineData(LocationChallengeStatus.Draft)]
    [InlineData(LocationChallengeStatus.Archived)]
    public void Draft_or_archived_location_challenge_can_be_deleted(LocationChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var deletion = () => challenge.Delete();

        deletion.Should().NotThrow();
    }

    [Theory]
    [InlineData(LocationChallengeStatus.Published)]
    [InlineData(LocationChallengeStatus.Retired)]
    public void Published_or_retired_location_challenge_cannot_be_deleted(LocationChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var deletion = () => challenge.Delete();

        deletion.Should().Throw<DomainException>();
    }

    // Walks a new challenge forward through the lifecycle, which follows the order of the
    // enum, until it reaches the given status.
    private static LocationChallenge CreateChallengeIn(LocationChallengeStatus status)
    {
        var challenge = new LocationChallenge("Potraga po tvrđavi", new Location(45.2517, 19.8619), 120,
            [new HiddenPoint(new Location(45.2531, 19.8625), Guid.NewGuid())]);
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

    private static Action GetTransition(LocationChallenge challenge, LocationChallengeStatus requestedStatus)
    {
        return requestedStatus switch
        {
            LocationChallengeStatus.Published => challenge.Publish,
            LocationChallengeStatus.Retired => challenge.Retire,
            LocationChallengeStatus.Archived => challenge.Archive,
            _ => throw new ArgumentOutOfRangeException(nameof(requestedStatus))
        };
    }
}
