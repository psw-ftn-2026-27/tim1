using FluentAssertions;
using Games.Domain.GeoPositions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.GeoPositions;

public sealed class GeoPositionTests
{
    [Theory]
    [InlineData(-91d, 19d)]
    [InlineData(91d, 19d)]
    [InlineData(45d, -181d)]
    [InlineData(45d, 181d)]
    [InlineData(double.NaN, 19d)]
    [InlineData(45d, double.NaN)]
    [InlineData(double.PositiveInfinity, 19d)]
    [InlineData(45d, double.NegativeInfinity)]
    public void A_position_outside_the_globe_or_without_a_finite_coordinate_is_rejected(double latitude, double longitude)
    {
        var create = () => new GeoPosition(latitude, longitude);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void The_distance_between_two_points_is_measured_in_metres_along_the_surface_of_the_earth()
    {
        var equator = new GeoPosition(0, 0);
        var oneDegreeNorth = new GeoPosition(1, 0);

        var distance = equator.DistanceTo(oneDegreeNorth);

        distance.Should().BeApproximately(111_195, 1);
    }
}
