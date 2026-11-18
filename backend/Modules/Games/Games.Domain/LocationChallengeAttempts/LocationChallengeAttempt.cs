using Shared.Domain;

namespace Games.Domain.LocationChallengeAttempts;

/// <summary>
/// An explorer's search for the hidden points of one location challenge. The search circle
/// is set once, at activation, so the explorer always sees the same circle in this attempt.
/// The rules that also need the challenge live in <see cref="LocationChallengeActivation"/>.
/// </summary>
public sealed class LocationChallengeAttempt : AggregateRoot
{
    public Guid ChallengeId { get; private set; }
    public Guid UserId { get; private set; }
    public SearchCircle Circle { get; private set; }
    public DateTime StartedAt { get; private set; }

    private LocationChallengeAttempt()
    {
        Circle = null!;
    }

    public LocationChallengeAttempt(Guid challengeId, Guid userId, SearchCircle circle)
        : base(Guid.NewGuid())
    {
        ChallengeId = challengeId;
        UserId = userId;
        Circle = circle;
        StartedAt = DateTime.UtcNow;
    }
}
