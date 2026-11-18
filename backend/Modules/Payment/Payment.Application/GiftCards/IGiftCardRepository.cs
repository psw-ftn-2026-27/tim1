using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCards;

public interface IGiftCardRepository
{
    Task<GiftCard?> GetByIdAsync(Guid id);
    void Add(GiftCard giftCard);
}
