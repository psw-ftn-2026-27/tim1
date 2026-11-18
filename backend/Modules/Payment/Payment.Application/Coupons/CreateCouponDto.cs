namespace Payment.Application.Coupons;

/// <summary>A missing product means the coupon is valid for any product of the owner.</summary>
public sealed record CreateCouponDto(
    Guid? ProductId,
    int Percent,
    DateTime ExpiresAt);
