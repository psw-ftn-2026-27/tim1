using FluentAssertions;
using Payment.Domain.Coupons;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Coupons;

public class CouponTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    public void Discount_outside_one_to_a_hundred_percent_is_refused(int percent)
    {
        var making = () => new Coupon(WellKnownUsers.Explorer, null, percent, DateTime.UtcNow.AddDays(7));

        making.Should().Throw<DomainException>();
    }

    [Fact]
    public void Coupon_that_expires_in_the_past_is_refused()
    {
        var making = () => new Coupon(WellKnownUsers.Explorer, null, 20, DateTime.UtcNow.AddDays(-1));

        making.Should().Throw<DomainException>();
    }

    [Fact]
    public void Unused_coupon_is_valid_until_it_expires_and_expired_after()
    {
        var coupon = new Coupon(WellKnownUsers.Explorer, null, 20, DateTime.UtcNow.AddDays(7));

        coupon.StatusAt(DateTime.UtcNow).Should().Be(CouponStatus.Valid);
        coupon.StatusAt(DateTime.UtcNow.AddDays(8)).Should().Be(CouponStatus.Expired);
    }

    [Fact]
    public void New_coupon_gets_a_code_of_eight_characters()
    {
        var coupon = new Coupon(WellKnownUsers.Explorer, null, 20, DateTime.UtcNow.AddDays(7));

        coupon.Code.Should().HaveLength(8);
    }
}
