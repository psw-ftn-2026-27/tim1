namespace Payment.Application.Checkout;

/// <summary>What the customer chose to pay with besides the wallet; both are optional.</summary>
public sealed record CheckoutDto(string? CouponCode, Guid? GiftCardId);
