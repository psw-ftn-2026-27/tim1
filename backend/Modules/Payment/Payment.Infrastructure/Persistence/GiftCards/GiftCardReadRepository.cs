using Microsoft.EntityFrameworkCore;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal sealed class GiftCardReadRepository : IGiftCardReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public GiftCardReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PageResult<GiftCardDto>> GetCreatedAsync(Guid creatorId, int page, int pageSize)
    {
        return GetPageAsync(_dbContext.GiftCards.Where(giftCard => giftCard.CreatorId == creatorId), page, pageSize);
    }

    public Task<PageResult<GiftCardDto>> GetReceivedAsync(Guid recipientId, int page, int pageSize)
    {
        return GetPageAsync(_dbContext.GiftCards.Where(giftCard => giftCard.RecipientId == recipientId), page, pageSize);
    }

    public Task<int> CountValidReceivedAsync(Guid recipientId)
    {
        var now = DateTime.UtcNow;

        return _dbContext.GiftCards.CountAsync(giftCard =>
            giftCard.RecipientId == recipientId && giftCard.UsedAt == null && giftCard.ExpiresAt >= now);
    }

    // Display names live in Identity, so they stay empty here and the queries class fills them in.
    // The status mirrors GiftCard.StatusAt, because a list is projected and never loads aggregates.
    private static async Task<PageResult<GiftCardDto>> GetPageAsync(IQueryable<GiftCard> giftCards, int page, int pageSize)
    {
        var now = DateTime.UtcNow;
        var totalCount = await giftCards.CountAsync();

        var items = await giftCards
            .OrderByDescending(giftCard => giftCard.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
                    : GiftCardStatus.Valid))
            .ToListAsync();

        return new PageResult<GiftCardDto>(items, totalCount);
    }
}
