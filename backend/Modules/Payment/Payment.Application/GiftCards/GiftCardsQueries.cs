using Identity.Contracts;
using Payment.Domain.GiftCards;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Application.GiftCards;

public sealed class GiftCardsQueries
{
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IIdentityApi _identityApi;

    public GiftCardsQueries(IGiftCardRepository giftCardRepository, IIdentityApi identityApi)
    {
        _giftCardRepository = giftCardRepository;
        _identityApi = identityApi;
    }

    public async Task<GiftCardsDto> GetGiftCardsAsync(Guid userId)
    {
        var giftCards = await _giftCardRepository.GetOfUserAsync(userId);

        var userIds = giftCards.SelectMany(card => new[] { card.CreatorId, card.RecipientId }).Distinct().ToList();
        var users = await _identityApi.GetUsersAsync(userIds);
        var displayNames = users.ToDictionary(user => user.Id, user => user.DisplayName);

        var now = DateTime.UtcNow;
        var dtos = giftCards
            .OrderByDescending(card => card.CreatedAt)
            .Select(card => new GiftCardDto(
                card.Id,
                card.CreatorId,
                displayNames.GetValueOrDefault(card.CreatorId, ""),
                card.RecipientId,
                displayNames.GetValueOrDefault(card.RecipientId, ""),
                card.Amount,
                card.Message,
                card.ExpiresAt,
                card.StatusAt(now)))
            .ToList();

        return new GiftCardsDto(
            [.. dtos.Where(dto => dto.CreatorId == userId)],
            [.. dtos.Where(dto => dto.RecipientId == userId)]);
    }

    public async Task<int> GetValidReceivedCountAsync(Guid userId)
    {
        var giftCards = await _giftCardRepository.GetOfUserAsync(userId);
        var now = DateTime.UtcNow;

        return giftCards.Count(card => card.RecipientId == userId && card.StatusAt(now) == GiftCardStatus.Valid);
    }

    public async Task<RecipientDto> GetRecipientAsync(Guid recipientId)
    {
        var users = await _identityApi.GetUsersAsync([recipientId]);
        var user = users.SingleOrDefault()
            ?? throw new NotFoundException($"User {recipientId} does not exist.");

        return new RecipientDto(user.Id, user.DisplayName);
    }

    public async Task<PageResult<RecipientDto>> SearchRecipientsAsync(string text, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = await _identityApi.SearchUsersAsync(text, page, pageSize);

        var items = users.Items.Select(user => new RecipientDto(user.Id, user.DisplayName)).ToList();
        return new PageResult<RecipientDto>(items, users.TotalCount);
    }
}
