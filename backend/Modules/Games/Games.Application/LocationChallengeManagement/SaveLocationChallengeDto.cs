namespace Games.Application.LocationChallengeManagement;

public sealed record SaveLocationChallengeDto(
    string Name,
    LocationDto StartLocation,
    int ExperiencePoints,
    IReadOnlyList<HiddenPointInputDto> HiddenPoints);

public sealed record HiddenPointInputDto(LocationDto Location, Guid ImageId);
