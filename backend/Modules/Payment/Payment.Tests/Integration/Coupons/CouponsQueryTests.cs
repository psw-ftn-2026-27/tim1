using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Coupons;
using Payment.Domain.Coupons;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
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

        var page = await client.GetFromJsonAsync<PageResult<CouponDto>>("/api/payment/coupons", JsonOptions);

        var coupons = page!.Items;
        var ownCoupons = CouponSeed.All.Where(coupon => coupon.OwnerId == DemoUsers.Explorer);
        coupons.Select(coupon => coupon.Id).Should().BeEquivalentTo(ownCoupons.Select(coupon => coupon.Id));
        coupons.Should().NotContain(coupon => coupon.Id == CouponSeed.ForgottenOwnersCoupon.Id);
    }

    [Fact]
    public async Task Owner_sees_the_products_they_own_and_not_those_of_others()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var products = await client.GetFromJsonAsync<List<OwnedProductDto>>("/api/payment/coupons/owned-products", JsonOptions);

        products!.Select(product => product.Id).Should().Contain(ProductSeed.DemoExplorersTour.Id);
        products.Should().NotContain(product => product.Id == ProductSeed.TourOfAForgottenOwner.Id);
    }

    [Fact]
    public async Task Coupon_shows_its_product_name_or_none_when_it_is_valid_for_any_product()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var page = await client.GetFromJsonAsync<PageResult<CouponDto>>("/api/payment/coupons", JsonOptions);

        var tourCoupon = page!.Items.Single(coupon => coupon.Id == CouponSeed.DemoExplorersTourCoupon.Id);
        tourCoupon.ProductName.Should().Be(ProductSeed.DemoExplorersTour.Name);
        tourCoupon.Status.Should().Be(CouponStatus.Valid);
        page.Items.Single(coupon => coupon.Id == CouponSeed.DemoExplorersAnyProductCoupon.Id)
            .ProductName.Should().BeNull();
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
