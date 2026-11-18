using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record Coordinate
{
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
}
