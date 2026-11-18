using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal sealed class GiftCardRepository : IGiftCardRepository
{
    private readonly PaymentDbContext _dbContext;

    public GiftCardRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(GiftCard giftCard)
    {
        _dbContext.GiftCards.Add(giftCard);
    }
}
