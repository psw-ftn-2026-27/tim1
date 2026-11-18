namespace Payment.Application.GiftCards;

public sealed record UsableGiftCardDto(Guid Id, decimal Amount, DateTime ExpiresAt);
