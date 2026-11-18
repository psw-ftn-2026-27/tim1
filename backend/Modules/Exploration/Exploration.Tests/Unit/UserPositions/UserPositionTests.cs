using Exploration.Domain.UserPositions;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.UserPositions;

public class UserPositionTests
{
    [Theory]
    [InlineData(-90.01, 20.0)]
    [InlineData(90.01, 20.0)]
    [InlineData(45.0, -180.01)]
    [InlineData(45.0, 180.01)]
    public void First_position_requires_coordinates_in_range(double latitude, double longitude)
    {
        var userId = WellKnownUsers.Explorer;

        var save = () => new UserPosition(userId, latitude, longitude);

        save.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-90.01, 20.0)]
    [InlineData(90.01, 20.0)]
    [InlineData(45.0, -180.01)]
    [InlineData(45.0, 180.01)]
    public void Rejected_position_change_preserves_the_previous_position(double latitude, double longitude)
    {
        var position = new UserPosition(WellKnownUsers.Explorer, 45.25, 19.83);
        var updatedAt = position.UpdatedAt;

        var change = () => position.ChangePosition(latitude, longitude);

        change.Should().Throw<DomainException>();
        position.Latitude.Should().Be(45.25);
        position.Longitude.Should().Be(19.83);
        position.UpdatedAt.Should().Be(updatedAt);
    }
}
