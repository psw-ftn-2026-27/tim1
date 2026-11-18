using Shared.Domain.Exceptions;

namespace Games.Domain.GeoPositions;

public sealed record GeoPosition
{
    private const double EarthRadiusMeters = 6_371_000;

    public double Latitude { get; }
    public double Longitude { get; }

    public GeoPosition(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || latitude is < -90 or > 90)
        {
            throw new DomainException("Latitude must be a finite number between -90 and 90 degrees.");
        }

        if (!double.IsFinite(longitude) || longitude is < -180 or > 180)
        {
            throw new DomainException("Longitude must be a finite number between -180 and 180 degrees.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>The haversine distance along the surface of the Earth, in metres.</summary>
    public double DistanceTo(GeoPosition other)
    {
        var fromLatitude = ToRadians(Latitude);
        var toLatitude = ToRadians(other.Latitude);
        var latitudeDifference = toLatitude - fromLatitude;
        var longitudeDifference = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(latitudeDifference / 2) * Math.Sin(latitudeDifference / 2)
            + Math.Cos(fromLatitude) * Math.Cos(toLatitude)
            * Math.Sin(longitudeDifference / 2) * Math.Sin(longitudeDifference / 2);

        return 2 * EarthRadiusMeters * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
