namespace Payment.Application.GiftCards;

public sealed record PurchaseGiftCardDto(Guid RecipientId, decimal Amount, string Message);
