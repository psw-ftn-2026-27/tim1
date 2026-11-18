namespace Payment.Application.Checkout;

public sealed record UsableGiftCardDto(Guid Id, decimal Amount, DateTime ExpiresAt);
