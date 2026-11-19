using FluentAssertions;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public sealed class LocationTests
{
    [Theory]
    [InlineData(-90.0001, 19.8451)]
    [InlineData(90.0001, 19.8451)]
    [InlineData(45.2551, -180.0001)]
    [InlineData(45.2551, 180.0001)]
    [InlineData(double.NaN, 19.8451)]
    [InlineData(45.2551, double.NaN)]
    public void Location_outside_the_latitude_and_longitude_ranges_is_not_created(double latitude, double longitude)
    {
        var creation = () => new Location(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-90.0, -180.0)]
    [InlineData(90.0, 180.0)]
    public void Location_on_the_edges_of_the_ranges_is_created(double latitude, double longitude)
    {
        var location = new Location(latitude, longitude);

        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    // Expected distances are geodesics on the WGS84 ellipsoid; the tolerance covers the
    // difference between the Earth and the sphere the distance is computed on.
    [Theory]
    [InlineData(48.8566, 2.3522, 51.5074, -0.1278, 343_923.1)]
    [InlineData(45.2551, 19.8451, 45.25515, 19.8451, 5.557)]
    public void Distance_between_two_known_locations_matches_the_real_distance(
        double fromLatitude, double fromLongitude, double toLatitude, double toLongitude, double expectedMeters)
    {
        var from = new Location(fromLatitude, fromLongitude);
        var to = new Location(toLatitude, toLongitude);

        var distance = from.DistanceTo(to);

        distance.Should().BeApproximately(expectedMeters, expectedMeters * 0.005);
    }
}
