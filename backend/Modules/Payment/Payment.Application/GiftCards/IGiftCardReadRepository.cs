namespace Payment.Application.GiftCards;

public interface IGiftCardReadRepository
{
    Task<List<GiftCardDto>> GetCreatedAsync(Guid creatorId);
    Task<List<GiftCardDto>> GetReceivedAsync(Guid recipientId);
    Task<int> CountValidReceivedAsync(Guid recipientId);
}
