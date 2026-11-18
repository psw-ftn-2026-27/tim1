using Games.Domain.LocationChallenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeAttempts;

/// <summary>
/// An explorer's search for the hidden points of one location challenge. The search circle
/// is set once, at activation, so the explorer always sees the same circle in this attempt.
/// The rules that also need the challenge live in <see cref="LocationChallengeActivation"/>
/// and <see cref="LocationDiscovery"/>.
/// </summary>
public sealed class LocationChallengeAttempt : AggregateRoot
{
    private readonly List<Guid> _discoveredPointIds = [];

    public Guid ChallengeId { get; private set; }
    public Guid UserId { get; private set; }
    public SearchCircle Circle { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? SolvedAt { get; private set; }
    public int? AwardedExperiencePoints { get; private set; }
    public IReadOnlyList<Guid> DiscoveredPointIds => _discoveredPointIds.AsReadOnly();
    public bool IsSolved => SolvedAt is not null;

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

    /// <summary>
    /// Records the discovered hidden points, in any order. When every hidden point of the
    /// challenge is discovered, the attempt is solved and the challenge's XP is awarded once.
    /// </summary>
    public void Discover(LocationChallenge challenge, IReadOnlyList<Guid> pointIds)
    {
        if (challenge.Id != ChallengeId)
        {
            throw new DomainException("An attempt can discover only the hidden points of its own challenge.");
        }
        if (IsSolved)
        {
            throw new DomainException("This location challenge is already solved.");
        }

        var challengePointIds = challenge.HiddenPoints.Select(point => point.Id).ToList();
        if (pointIds.Any(pointId => !challengePointIds.Contains(pointId)))
        {
            throw new DomainException("An attempt can discover only the hidden points of its own challenge.");
        }

        _discoveredPointIds.AddRange(pointIds.Where(pointId => !_discoveredPointIds.Contains(pointId)).Distinct());

        if (challengePointIds.All(_discoveredPointIds.Contains))
        {
            SolvedAt = DateTime.UtcNow;
            AwardedExperiencePoints = challenge.ExperiencePoints;
        }
    }
}
