using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed record Location
{
    // Mean radius of the Earth. Treating the Earth as a sphere is off by at most about 0.5%,
    // which is far below the tolerance of a position chosen on a map.
    private const double EarthRadiusMeters = 6_371_000;

    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Location() { }

    public Location(double latitude, double longitude)
    {
        if (latitude is not (>= -90 and <= 90))
        {
            throw new DomainException("A location requires a latitude between -90 and 90.");
        }
        if (longitude is not (>= -180 and <= 180))
        {
            throw new DomainException("A location requires a longitude between -180 and 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>The distance in meters along the Earth's surface (haversine formula).</summary>
    public double DistanceTo(Location other)
    {
        var latitudeDelta = double.DegreesToRadians(other.Latitude - Latitude);
        var longitudeDelta = double.DegreesToRadians(other.Longitude - Longitude);

        var halfChord = Math.Pow(Math.Sin(latitudeDelta / 2), 2)
            + Math.Cos(double.DegreesToRadians(Latitude)) * Math.Cos(double.DegreesToRadians(other.Latitude))
            * Math.Pow(Math.Sin(longitudeDelta / 2), 2);

        return 2 * EarthRadiusMeters * Math.Asin(Math.Min(1, Math.Sqrt(halfChord)));
    }
}
