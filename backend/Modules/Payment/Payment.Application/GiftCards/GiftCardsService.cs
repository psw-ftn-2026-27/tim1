using Identity.Contracts;
using Payment.Application.Wallets;
using Payment.Domain.GiftCards;
using Shared.Domain.Exceptions;

namespace Payment.Application.GiftCards;

public sealed class GiftCardsService
{
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IIdentityApi _identityApi;
    private readonly IUnitOfWork _unitOfWork;

    public GiftCardsService(
        IGiftCardRepository giftCardRepository,
        IWalletRepository walletRepository,
        IIdentityApi identityApi,
        IUnitOfWork unitOfWork)
    {
        _giftCardRepository = giftCardRepository;
        _walletRepository = walletRepository;
        _identityApi = identityApi;
        _unitOfWork = unitOfWork;
    }

    public async Task PurchaseGiftCardAsync(Guid buyerId, PurchaseGiftCardDto dto)
    {
        var recipients = await _identityApi.GetUsersAsync([dto.RecipientId]);
        if (recipients.Count == 0)
        {
            throw new NotFoundException($"User {dto.RecipientId} does not exist.");
        }

        var wallet = await _walletRepository.GetByOwnerAsync(buyerId);
        if (wallet is null)
        {
            throw new DomainException("Nemate dovoljno EC u novčaniku.");
        }

        wallet.PayForGiftCard(dto.Amount);
        _giftCardRepository.Add(new GiftCard(buyerId, dto.RecipientId, dto.Amount, dto.Message));
        await _unitOfWork.SaveChangesAsync();
    }
}
