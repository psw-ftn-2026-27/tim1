namespace Payment.Application.Checkout;

/// <summary>A missing coupon product name means the coupon was not used.</summary>
public sealed record PriceDto(
    decimal Total,
    decimal CouponDiscount,
    string? CouponProductName,
    decimal CardDiscount,
    decimal FinalPrice,
    decimal CardLost);
