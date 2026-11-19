using Games.Application.LocationChallenges;

namespace Games.Application.LocationChallengeSolving;

/// <summary>
/// What the explorer sees while searching: the circle and the photos, never the hidden points' locations.
/// SolvedAt and AwardedExperiencePoints (from the challenge completion) are null until the attempt is solved.
/// </summary>
public sealed record LocationChallengeAttemptDto(
    Guid Id,
    Guid ChallengeId,
    string ChallengeName,
    SearchCircleDto Circle,
    IReadOnlyList<AttemptPhotoDto> Photos,
    int RemainingPointCount,
    bool IsSolved,
    DateTime StartedAt,
    DateTime? SolvedAt,
    int? AwardedExperiencePoints);

public sealed record SearchCircleDto(LocationDto Center, double DiameterMeters);

public sealed record AttemptPhotoDto(Guid PointId, Guid ImageId, bool IsDiscovered);
