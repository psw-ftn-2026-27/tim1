using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCards;

public interface IGiftCardRepository
{
    Task<List<GiftCard>> GetOfUserAsync(Guid userId);
    void Add(GiftCard giftCard);
}
