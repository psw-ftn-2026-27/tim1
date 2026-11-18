using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Purchases;

/// <summary>
/// Prices a cart with an optional coupon and an optional gift card. The coupon lowers one item
/// of the cart by its percent, then the card lowers what is left by its whole amount, but not
/// below zero. Neither is spent here, so the cart view and the checkout price a cart the same way.
/// </summary>
public sealed class CartPricing
{
    public PriceBreakdown Calculate(
        ShoppingCart cart,
        IReadOnlyList<Product> products,
        Coupon? coupon,
        GiftCard? giftCard,
        DateTime now)
    {
        var total = cart.Total();

        var couponDiscount = 0m;
        ProductReference? couponReference = null;
        if (coupon is not null)
        {
            coupon.EnsureValidAt(now);

            var discountedItem = FindDiscountedItem(cart, products, coupon);
            couponReference = discountedItem.Reference;
            couponDiscount = Math.Round(discountedItem.Price * coupon.Percent / 100m, 2, MidpointRounding.AwayFromZero);
        }

        var afterCoupon = total - couponDiscount;

        var cardDiscount = 0m;
        var cardLost = 0m;
        if (giftCard is not null)
        {
            giftCard.EnsureUsableBy(cart.OwnerId, now);

            cardDiscount = Math.Min(giftCard.Amount, afterCoupon);
            cardLost = giftCard.Amount - cardDiscount;
        }

        return new PriceBreakdown(total, couponDiscount, cardDiscount, afterCoupon - cardDiscount, cardLost, couponReference);
    }

    /// <summary>
    /// The product the coupon is tied to, or the dearest item of the coupon's owner when it is tied
    /// to none; a cart without such an item cannot use the coupon.
    /// </summary>
    private static OrderItem FindDiscountedItem(ShoppingCart cart, IReadOnlyList<Product> products, Coupon coupon)
    {
        OrderItem? item;
        if (coupon.ProductId is not null)
        {
            var product = products.FirstOrDefault(product => product.Id == coupon.ProductId);
            item = cart.Items.FirstOrDefault(cartItem => cartItem.Reference == product?.Reference);
        }
        else
        {
            var ownerReferences = products
                .Where(product => product.OwnerId == coupon.OwnerId)
                .Select(product => product.Reference)
                .ToList();
            item = cart.Items
                .Where(cartItem => ownerReferences.Contains(cartItem.Reference))
                .OrderByDescending(cartItem => cartItem.Price)
                .FirstOrDefault();
        }

        return item ?? throw new DomainException("The coupon does not apply to anything in your cart.");
    }
}
