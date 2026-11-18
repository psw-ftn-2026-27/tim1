using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCards;

public sealed record GiftCardDto(
    Guid Id,
    Guid CreatorId,
    string CreatorDisplayName,
    Guid RecipientId,
    string RecipientDisplayName,
    decimal Amount,
    string Message,
    DateTime ExpiresAt,
    GiftCardStatus Status);
