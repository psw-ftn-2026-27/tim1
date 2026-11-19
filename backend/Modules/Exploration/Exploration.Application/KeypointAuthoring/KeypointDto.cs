namespace Exploration.Application.KeypointAuthoring;

public sealed record KeypointDto(
    Guid Id,
    string Name,
    string Description,
    Guid ImageId,
    string? Secret,
    CoordinateDto Location,
    bool PubliclyVisible);
