namespace Exploration.Application.KeypointAuthoring;

public sealed record AddKeypointDto(
    string Name,
    string Description,
    Guid ImageId,
    string? Secret,
    CoordinateDto Location,
    double LengthKm,
    bool PubliclyVisible = false);
