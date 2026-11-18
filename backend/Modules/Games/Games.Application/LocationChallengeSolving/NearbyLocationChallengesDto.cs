using Games.Application.LocationChallenges;

namespace Games.Application.LocationChallengeSolving;

/// <summary>The explorer's position and the challenges around it; Position is null when the explorer has not saved one.</summary>
public sealed record NearbyLocationChallengesDto(
    LocationDto? Position,
    IReadOnlyList<NearbyLocationChallengeDto> Challenges);

public sealed record NearbyLocationChallengeDto(
    Guid Id,
    string Name,
    int ExperiencePoints,
    double DistanceMeters,
    bool CanActivate);

public sealed record PublishedLocationChallengeDto(
    Guid Id,
    string Name,
    LocationDto StartLocation,
    int ExperiencePoints);
