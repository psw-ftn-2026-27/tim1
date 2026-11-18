using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record Coordinate
{
    private const double EarthRadiusKm = 6371.0;

    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Coordinate() { }

    public Coordinate(double latitude, double longitude)
    {
        if (double.IsNaN(latitude) || latitude < -90 || latitude > 90)
        {
            throw new DomainException("Latitude must be between -90 and 90.");
        }
        if (double.IsNaN(longitude) || longitude < -180 || longitude > 180)
        {
            throw new DomainException("Longitude must be between -180 and 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    public double DistanceTo(Coordinate other)
    {
        var latitudeDifference = ToRadians(other.Latitude - Latitude);
        var longitudeDifference = ToRadians(other.Longitude - Longitude);
        var a = Math.Sin(latitudeDifference / 2) * Math.Sin(latitudeDifference / 2)
            + Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude))
            * Math.Sin(longitudeDifference / 2) * Math.Sin(longitudeDifference / 2);

        return EarthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
