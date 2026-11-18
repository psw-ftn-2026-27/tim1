using Microsoft.EntityFrameworkCore;
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

    public Task<GiftCard?> GetByIdAsync(Guid id)
    {
        return _dbContext.GiftCards.FirstOrDefaultAsync(giftCard => giftCard.Id == id);
    }

    public void Add(GiftCard giftCard)
    {
        _dbContext.GiftCards.Add(giftCard);
    }
}
