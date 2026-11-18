using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Coupons;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Coupons;

public class CouponsCommandTests : BaseIntegrationTest
{
    public CouponsCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_makes_a_coupon_for_their_product_and_it_is_stored_with_a_generated_code()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");
        var dto = new CreateCouponDto(ProductSeed.DemoExplorersTour.Id, 15, DateTime.UtcNow.AddDays(14));
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponCountBefore = arrangeContext.Coupons.Count(coupon => coupon.OwnerId == DemoUsers.Explorer);

        var response = await client.PostAsJsonAsync("/api/payment/coupons", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedCoupons = assertContext.Coupons.Where(coupon => coupon.OwnerId == DemoUsers.Explorer).ToList();
        storedCoupons.Should().HaveCount(couponCountBefore + 1);
        var newCoupon = storedCoupons.Single(coupon =>
            coupon.Id != CouponSeed.DemoExplorersTourCoupon.Id
            && coupon.Id != CouponSeed.DemoExplorersAnyProductCoupon.Id);
        newCoupon.ProductId.Should().Be(ProductSeed.DemoExplorersTour.Id);
        newCoupon.Percent.Should().Be(15);
        newCoupon.Code.Should().HaveLength(8);
        newCoupon.UsedAt.Should().BeNull();
    }

    [Fact]
    public async Task Owner_makes_a_coupon_for_any_of_their_products_and_it_is_stored_without_a_product()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");
        var dto = new CreateCouponDto(null, 25, DateTime.UtcNow.AddDays(14));
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponCountBefore = arrangeContext.Coupons.Count(coupon => coupon.OwnerId == DemoUsers.Explorer);

        var response = await client.PostAsJsonAsync("/api/payment/coupons", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedCoupons = assertContext.Coupons.Where(coupon => coupon.OwnerId == DemoUsers.Explorer).ToList();
        storedCoupons.Should().HaveCount(couponCountBefore + 1);
        var newCoupon = storedCoupons.Single(coupon => coupon.Percent == 25);
        newCoupon.ProductId.Should().BeNull();
    }

    [Fact]
    public async Task Coupon_tied_to_a_product_of_another_owner_is_refused_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");
        var dto = new CreateCouponDto(ProductSeed.TourOfAForgottenOwner.Id, 15, DateTime.UtcNow.AddDays(14));
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponCountBefore = arrangeContext.Coupons.Count();

        var response = await client.PostAsJsonAsync("/api/payment/coupons", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Count().Should().Be(couponCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_make_a_coupon()
    {
        var client = Factory.CreateClient();
        var dto = new CreateCouponDto(null, 15, DateTime.UtcNow.AddDays(14));

        var response = await client.PostAsJsonAsync("/api/payment/coupons", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_make_a_coupon()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var dto = new CreateCouponDto(null, 15, DateTime.UtcNow.AddDays(14));

        var response = await client.PostAsJsonAsync("/api/payment/coupons", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
