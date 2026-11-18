using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Purchases;

public class CartPricingTests
{
    private static readonly ProductReference FortressTour =
        new(ProductType.Tour, Guid.Parse("40000000-0000-0000-0000-000000000001"));

    private static readonly ProductReference MountainRide =
        new(ProductType.Tour, Guid.Parse("40000000-0000-0000-0000-000000000002"));

    private static readonly ProductReference CityWalk =
        new(ProductType.Tour, Guid.Parse("40000000-0000-0000-0000-000000000003"));

    private static readonly Guid Buyer = WellKnownUsers.Explorer;
    private static readonly Guid Seller = WellKnownUsers.SecondExplorer;
    private static readonly Guid OtherSeller = Guid.Parse("90000000-0000-0000-0000-000000000001");

    private static readonly Product Fortress = new(FortressTour, "Walk around the Petrovaradin fortress", 1200m, Seller);
    private static readonly Product Ride = new(MountainRide, "Ride across Fruska gora", 900m, Seller);
    private static readonly Product Walk = new(CityWalk, "Walk through the old town", 2000m, OtherSeller);

    private static List<Product> CreateProducts() => [Fortress, Ride, Walk];

    private static ShoppingCart CreateCartWithAllTours()
    {
        var cart = new ShoppingCart(Buyer);
        cart.AddItem(FortressTour, Fortress.Name, Fortress.Price);
        cart.AddItem(MountainRide, Ride.Name, Ride.Price);
        cart.AddItem(CityWalk, Walk.Name, Walk.Price);
        return cart;
    }

    private static ShoppingCart CreateCartWithTheRideAlone()
    {
        var cart = new ShoppingCart(Buyer);
        cart.AddItem(MountainRide, Ride.Name, Ride.Price);
        return cart;
    }

    private static Coupon CreateCoupon(Guid? productId, int percent) =>
        new(Seller, productId, percent, DateTime.UtcNow.AddDays(7));

    private static GiftCard CreateGiftCard(decimal amount) =>
        new(Seller, Buyer, amount, "Enjoy the tours!");

    [Fact]
    public void Coupon_tied_to_a_product_lowers_the_price_of_that_product_by_its_percent()
    {
        var cart = CreateCartWithAllTours();
        var coupon = CreateCoupon(Ride.Id, 25);

        var price = new CartPricing().Calculate(cart, CreateProducts(), coupon, null, DateTime.UtcNow);

        price.CouponReference.Should().Be(MountainRide);
        price.CouponDiscount.Should().Be(225m);
        price.Total.Should().Be(4100m);
        price.FinalPrice.Should().Be(3875m);
    }

    [Fact]
    public void Coupon_tied_to_no_product_lowers_the_dearest_item_of_its_owner()
    {
        var cart = CreateCartWithAllTours();
        var coupon = CreateCoupon(null, 10);

        var price = new CartPricing().Calculate(cart, CreateProducts(), coupon, null, DateTime.UtcNow);

        price.CouponReference.Should().Be(FortressTour);
        price.CouponDiscount.Should().Be(120m);
        price.FinalPrice.Should().Be(3980m);
    }

    [Fact]
    public void Coupon_for_a_product_that_is_not_in_the_cart_does_not_apply()
    {
        var cart = CreateCartWithTheRideAlone();
        var coupon = CreateCoupon(Fortress.Id, 25);

        var pricing = () => new CartPricing().Calculate(cart, CreateProducts(), coupon, null, DateTime.UtcNow);

        pricing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Gift_card_lowers_the_price_of_the_cart_after_the_coupon()
    {
        var cart = CreateCartWithAllTours();
        var coupon = CreateCoupon(Ride.Id, 25);
        var giftCard = CreateGiftCard(100m);

        var price = new CartPricing().Calculate(cart, CreateProducts(), coupon, giftCard, DateTime.UtcNow);

        price.CardDiscount.Should().Be(100m);
        price.CardLost.Should().Be(0);
        price.FinalPrice.Should().Be(3775m);
    }

    [Fact]
    public void Gift_card_above_the_price_of_the_cart_makes_it_free_and_the_rest_of_the_card_is_lost()
    {
        var cart = CreateCartWithTheRideAlone();
        var giftCard = CreateGiftCard(1000m);

        var price = new CartPricing().Calculate(cart, CreateProducts(), null, giftCard, DateTime.UtcNow);

        price.CardDiscount.Should().Be(900m);
        price.FinalPrice.Should().Be(0);
        price.CardLost.Should().Be(100m);
    }

    [Fact]
    public void Coupon_that_has_expired_is_not_priced()
    {
        var cart = CreateCartWithAllTours();
        var coupon = CreateCoupon(Ride.Id, 25);

        var pricing = () => new CartPricing().Calculate(cart, CreateProducts(), coupon, null, DateTime.UtcNow.AddDays(8));

        pricing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Gift_card_that_has_expired_is_not_priced()
    {
        var cart = CreateCartWithTheRideAlone();
        var giftCard = CreateGiftCard(100m);

        var pricing = () => new CartPricing().Calculate(cart, CreateProducts(), null, giftCard, giftCard.ExpiresAt.AddDays(1));

        pricing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Coupon_discount_is_rounded_to_cents_away_from_zero()
    {
        var cheapTour = new Product(CityWalk, "Walk through the old town", 12.50m, Seller);
        var cart = new ShoppingCart(Buyer);
        cart.AddItem(CityWalk, cheapTour.Name, cheapTour.Price);
        var coupon = CreateCoupon(cheapTour.Id, 25);

        var price = new CartPricing().Calculate(cart, [cheapTour], coupon, null, DateTime.UtcNow);

        price.CouponDiscount.Should().Be(3.13m);
    }

    [Fact]
    public void Coupon_of_a_hundred_percent_makes_its_item_free()
    {
        var cart = CreateCartWithTheRideAlone();
        var coupon = CreateCoupon(Ride.Id, 100);

        var price = new CartPricing().Calculate(cart, CreateProducts(), coupon, null, DateTime.UtcNow);

        price.FinalPrice.Should().Be(0);
    }

    [Fact]
    public void Gift_card_equal_to_the_price_of_the_cart_makes_it_free_and_loses_nothing()
    {
        var cart = CreateCartWithTheRideAlone();
        var giftCard = CreateGiftCard(900m);

        var price = new CartPricing().Calculate(cart, CreateProducts(), null, giftCard, DateTime.UtcNow);

        price.FinalPrice.Should().Be(0);
        price.CardLost.Should().Be(0);
    }
}
