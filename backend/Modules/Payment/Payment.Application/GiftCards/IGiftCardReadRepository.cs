using Payment.Application.Checkout;
using Shared.Domain;

namespace Payment.Application.GiftCards;

public interface IGiftCardReadRepository
{
    Task<PageResult<GiftCardDto>> GetCreatedAsync(Guid creatorId, int page, int pageSize);
    Task<PageResult<GiftCardDto>> GetReceivedAsync(Guid recipientId, int page, int pageSize);
    Task<int> CountValidReceivedAsync(Guid recipientId);
    Task<List<UsableGiftCardDto>> GetValidReceivedAsync(Guid recipientId);
}
