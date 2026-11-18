using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Checkout;
using Payment.Tests.Integration.Seeds;
using Xunit;

namespace Payment.Tests.Integration.Checkout;

public class CheckoutQueryTests : BaseIntegrationTest
{
    public CheckoutQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Price_of_the_cart_shows_the_coupon_and_the_gift_card_taken_off()
    {
        var client = Factory.CreateClientFor(DemoUsers.SecondExplorer, "explorer");
        var coupon = CouponSeed.DemoExplorersTourCoupon;
        var giftCard = GiftCardSeed.ExplorersGiftCardForSecondExplorer;
        var total = ShoppingCartSeed.BuyersCart.Total();
        var couponDiscount = ProductSeed.DemoExplorersTour.Price * coupon.Percent / 100m;

        var price = await client.GetFromJsonAsync<PriceDto>(
            $"/api/payment/cart/price?couponCode={coupon.Code}&giftCardId={giftCard.Id}", JsonOptions);

        price!.Total.Should().Be(total);
        price.CouponDiscount.Should().Be(couponDiscount);
        price.CouponProductName.Should().Be(ProductSeed.DemoExplorersTour.Name);
        price.CardDiscount.Should().Be(giftCard.Amount);
        price.FinalPrice.Should().Be(total - couponDiscount - giftCard.Amount);
        price.CardLost.Should().Be(0);
    }

    [Fact]
    public async Task Customer_sees_only_their_own_valid_gift_cards_to_pay_with()
    {
        var client = Factory.CreateClientFor(DemoUsers.Explorer, "explorer");

        var giftCards = await client.GetFromJsonAsync<List<UsableGiftCardDto>>(
            "/api/payment/cart/usable-gift-cards", JsonOptions);

        giftCards!.Should().Contain(giftCard =>
            giftCard.Id == GiftCardSeed.SecondExplorersGiftCardForExplorer.Id
            && giftCard.Amount == GiftCardSeed.SecondExplorersGiftCardForExplorer.Amount);
        giftCards.Should().NotContain(giftCard => giftCard.Id == GiftCardSeed.ThirdExplorersUsedGiftCardForExplorer.Id);
        giftCards.Should().NotContain(giftCard => giftCard.Id == GiftCardSeed.ExplorersGiftCardForSecondExplorer.Id);
    }
}
