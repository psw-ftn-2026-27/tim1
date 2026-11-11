using FluentAssertions;
using Payment.Domain.TopUpRequests;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.TopUpRequests;

public class TopUpRequestTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2500)]
    public void Top_up_request_requires_an_amount_greater_than_zero(decimal amount)
    {
        var creation = () => new TopUpRequest(Guid.NewGuid(), amount);

        creation.Should().Throw<DomainException>();
    }
}
