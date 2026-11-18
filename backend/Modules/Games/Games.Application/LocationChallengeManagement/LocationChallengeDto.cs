using Games.Domain.LocationChallenges;

namespace Games.Application.LocationChallengeManagement;

public sealed record LocationChallengeDto(
    Guid Id,
    string Name,
    LocationDto StartLocation,
    int ExperiencePoints,
    LocationChallengeStatus Status,
    IReadOnlyList<HiddenPointDto> HiddenPoints);

public sealed record HiddenPointDto(Guid Id, LocationDto Location, Guid ImageId);

public sealed record LocationChallengeSummaryDto(
    Guid Id,
    string Name,
    int HiddenPointCount,
    int ExperiencePoints,
    LocationChallengeStatus Status);

public sealed record LocationDto(double Latitude, double Longitude);
