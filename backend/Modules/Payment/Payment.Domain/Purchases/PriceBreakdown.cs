using Payment.Domain.Products;

namespace Payment.Domain.Purchases;

/// <summary>
/// What a cart costs once the coupon and the gift card are counted: the total, what each took
/// off, the price to pay and how much of the card is lost because the cart cost less than the
/// card held. CouponReference names the item the coupon lowered, and is empty without a coupon.
/// </summary>
public sealed record PriceBreakdown(
    decimal Total,
    decimal CouponDiscount,
    decimal CardDiscount,
    decimal FinalPrice,
    decimal CardLost,
    ProductReference? CouponReference);
