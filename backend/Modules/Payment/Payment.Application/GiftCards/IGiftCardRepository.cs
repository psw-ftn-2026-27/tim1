using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCards;

public interface IGiftCardRepository
{
    void Add(GiftCard giftCard);
}
