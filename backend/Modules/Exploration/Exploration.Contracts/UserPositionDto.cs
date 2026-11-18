namespace Exploration.Contracts;

/// <summary>
/// The user's last saved latitude, longitude, and update time in UTC.
/// </summary>
public sealed record UserPositionDto(double Latitude, double Longitude, DateTime UpdatedAt);
