namespace Exploration.Application.UserPositioning;

public sealed record CurrentPositionDto(double Latitude, double Longitude, DateTime UpdatedAt);
