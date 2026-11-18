using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Application.GiftCards;

public sealed class GiftCardsQueries
{
    private readonly IGiftCardReadRepository _giftCardReadRepository;
    private readonly IIdentityApi _identityApi;

    public GiftCardsQueries(IGiftCardReadRepository giftCardReadRepository, IIdentityApi identityApi)
    {
        _giftCardReadRepository = giftCardReadRepository;
        _identityApi = identityApi;
    }

    public async Task<PageResult<GiftCardDto>> GetCreatedGiftCardsAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var created = await _giftCardReadRepository.GetCreatedAsync(userId, page, pageSize);

        return await WithDisplayNamesAsync(created);
    }

    public async Task<PageResult<GiftCardDto>> GetReceivedGiftCardsAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var received = await _giftCardReadRepository.GetReceivedAsync(userId, page, pageSize);

        return await WithDisplayNamesAsync(received);
    }

    public Task<int> GetValidReceivedCountAsync(Guid userId)
    {
        return _giftCardReadRepository.CountValidReceivedAsync(userId);
    }

    public async Task<RecipientDto> GetRecipientAsync(Guid recipientId)
    {
        var users = await _identityApi.GetUsersAsync([recipientId]);
        var user = users.SingleOrDefault()
            ?? throw new NotFoundException("The user does not exist.");

        return new RecipientDto(user.Id, user.DisplayName);
    }

    public async Task<PageResult<RecipientDto>> SearchRecipientsAsync(Guid userId, string text, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var users = await _identityApi.SearchUsersAsync(text, page, pageSize);

        var items = users.Items
            .Where(user => user.Id != userId)
            .Select(user => new RecipientDto(user.Id, user.DisplayName))
            .ToList();
        return new PageResult<RecipientDto>(items, users.TotalCount);
    }

    private async Task<PageResult<GiftCardDto>> WithDisplayNamesAsync(PageResult<GiftCardDto> giftCards)
    {
        var userIds = giftCards.Items
            .SelectMany(card => new[] { card.CreatorId, card.RecipientId })
            .Distinct()
            .ToList();
        var users = await _identityApi.GetUsersAsync(userIds);
        var displayNames = users.ToDictionary(user => user.Id, user => user.DisplayName);

        var items = giftCards.Items
            .Select(card => card with
            {
                CreatorDisplayName = displayNames.GetValueOrDefault(card.CreatorId, ""),
                RecipientDisplayName = displayNames.GetValueOrDefault(card.RecipientId, "")
            })
            .ToList();
        return new PageResult<GiftCardDto>(items, giftCards.TotalCount);
    }
}
