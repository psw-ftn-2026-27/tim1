using FluentAssertions;
using Payment.Domain.TopUpRequests;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.TopUpRequests;

public class TopUpRequestTests
{
    private static TopUpRequest CreatePendingRequest() => new(Guid.NewGuid(), 1500m);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2500)]
    public void Top_up_request_requires_an_amount_greater_than_zero(decimal amount)
    {
        var creation = () => new TopUpRequest(Guid.NewGuid(), amount);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Request_that_has_been_decided_cannot_be_decided_again()
    {
        var request = CreatePendingRequest();
        request.Approve();

        var decision = () => request.Approve();

        decision.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Request_cannot_be_rejected_without_a_reason(string reason)
    {
        var request = CreatePendingRequest();

        var rejection = () => request.Reject(reason);

        rejection.Should().Throw<DomainException>();
    }
}
