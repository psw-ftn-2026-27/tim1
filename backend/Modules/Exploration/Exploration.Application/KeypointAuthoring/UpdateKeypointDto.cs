namespace Exploration.Application.KeypointAuthoring;

public sealed record UpdateKeypointDto(
    string Name,
    string Description,
    Guid ImageId,
    string? Secret,
    CoordinateDto Location,
    double LengthKm);
