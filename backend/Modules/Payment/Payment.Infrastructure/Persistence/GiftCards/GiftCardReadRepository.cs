using Microsoft.EntityFrameworkCore;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal sealed class GiftCardReadRepository : IGiftCardReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public GiftCardReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<GiftCardDto>> GetCreatedAsync(Guid creatorId)
    {
        return ProjectToDtos(_dbContext.GiftCards.Where(giftCard => giftCard.CreatorId == creatorId)).ToListAsync();
    }

    public Task<List<GiftCardDto>> GetReceivedAsync(Guid recipientId)
    {
        return ProjectToDtos(_dbContext.GiftCards.Where(giftCard => giftCard.RecipientId == recipientId)).ToListAsync();
    }

    public Task<int> CountValidReceivedAsync(Guid recipientId)
    {
        var now = DateTime.UtcNow;

        return _dbContext.GiftCards.CountAsync(giftCard =>
            giftCard.RecipientId == recipientId && giftCard.UsedAt == null && giftCard.ExpiresAt >= now);
    }

    // Display names live in Identity, so they stay empty here and the queries class fills them in.
    // The status mirrors GiftCard.StatusAt, because a list is projected and never loads aggregates.
    private static IQueryable<GiftCardDto> ProjectToDtos(IQueryable<GiftCard> giftCards)
    {
        var now = DateTime.UtcNow;

        return giftCards
            .OrderByDescending(giftCard => giftCard.CreatedAt)
            .Select(giftCard => new GiftCardDto(
                giftCard.Id,
                giftCard.CreatorId,
                "",
                giftCard.RecipientId,
                "",
                giftCard.Amount,
                giftCard.Message,
                giftCard.ExpiresAt,
                giftCard.UsedAt != null ? GiftCardStatus.Used
                    : giftCard.ExpiresAt < now ? GiftCardStatus.Expired
                    : GiftCardStatus.Valid));
    }
}
