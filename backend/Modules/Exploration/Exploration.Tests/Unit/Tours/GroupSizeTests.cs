using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class GroupSizeTests
{
    [Theory]
    [InlineData(0, 5)]
    [InlineData(-1, 5)]
    [InlineData(5, 4)]
    public void Group_size_must_have_a_valid_range_of_visitors(int min, int max)
    {
        var creation = () => new GroupSize(min, max);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Group_size_is_created_with_a_valid_range()
    {
        var groupSize = new GroupSize(2, 10);

        groupSize.Min.Should().Be(2);
        groupSize.Max.Should().Be(10);
    }
}
