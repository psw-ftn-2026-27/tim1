namespace Exploration.Application.KeypointAuthoring;

public sealed record TourManagementDto(
    Guid Id,
    string Name,
    string Description,
    double Length,
    List<KeypointDto> Keypoints);
