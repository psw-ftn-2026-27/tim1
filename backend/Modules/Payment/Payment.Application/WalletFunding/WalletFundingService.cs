using Payment.Application.TopUpRequests;
using Payment.Application.Wallets;
using Payment.Domain.TopUpRequests;
using Payment.Domain.Wallets;

namespace Payment.Application.WalletFunding;

public sealed class WalletFundingService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITopUpRequestRepository _topUpRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletFundingService(
        IWalletRepository walletRepository,
        ITopUpRequestRepository topUpRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _topUpRequestRepository = topUpRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task RequestTopUpAsync(Guid ownerId, RequestTopUpDto dto)
    {
        var wallet = await _walletRepository.GetByOwnerAsync(ownerId);
        if (wallet is null)
        {
            wallet = new Wallet(ownerId);
            _walletRepository.Add(wallet);
        }

        _topUpRequestRepository.Add(new TopUpRequest(wallet.Id, dto.Amount));
        await _unitOfWork.SaveChangesAsync();
    }
}
