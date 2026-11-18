using Games.Application.LocationChallenges;

namespace Games.Application.LocationChallengeSolving;

/// <summary>What the explorer sees while searching: the circle and the photos, never the hidden points' locations.</summary>
public sealed record LocationChallengeAttemptDto(
    Guid Id,
    Guid ChallengeId,
    string ChallengeName,
    SearchCircleDto Circle,
    IReadOnlyList<Guid> ImageIds,
    DateTime StartedAt);

public sealed record SearchCircleDto(LocationDto Center, double DiameterMeters);
