namespace Payment.Application.Coupons;

/// <summary>A missing product name means the coupon is valid for any product of the owner.</summary>
public sealed record CouponDto(
    Guid Id,
    string Code,
    int Percent,
    DateTime ExpiresAt,
    string? ProductName,
    bool Used);
