using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Coupons;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Coupons;

public class CouponsQueryTests : BaseIntegrationTest
{
    public CouponsQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_sees_the_coupons_they_made_and_not_those_of_others()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var coupons = await client.GetFromJsonAsync<List<CouponDto>>("/api/payment/coupons", JsonOptions);

        var ownCoupons = CouponSeed.All.Where(coupon => coupon.OwnerId == DemoUsers.Explorer);
        coupons!.Select(coupon => coupon.Id).Should().BeEquivalentTo(ownCoupons.Select(coupon => coupon.Id));
        coupons.Should().NotContain(coupon => coupon.Id == CouponSeed.ForgottenOwnersCoupon.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_coupons()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/coupons");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_see_coupons()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/coupons");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
