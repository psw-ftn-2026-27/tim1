using Payment.Application.TopUpRequests;
using Payment.Application.Wallets;
using Payment.Application.WithdrawalRequests;
using Payment.Domain.TopUpRequests;
using Payment.Domain.Wallets;
using Payment.Domain.WithdrawalRequests;
using Shared.Domain.Exceptions;

namespace Payment.Application.WalletFunding;

public sealed class WalletFundingService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITopUpRequestRepository _topUpRequestRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletFundingService(
        IWalletRepository walletRepository,
        ITopUpRequestRepository topUpRequestRepository,
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _topUpRequestRepository = topUpRequestRepository;
        _withdrawalRequestRepository = withdrawalRequestRepository;
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

    public async Task RequestWithdrawalAsync(Guid ownerId, RequestWithdrawalDto dto)
    {
        var wallet = await _walletRepository.GetByOwnerAsync(ownerId);
        if (wallet is null)
        {
            throw new NotFoundException($"Wallet of owner {ownerId} does not exist.");
        }

        wallet.ReserveForWithdrawal(dto.Amount);
        _withdrawalRequestRepository.Add(new WithdrawalRequest(wallet.Id, dto.Amount));
        await _unitOfWork.SaveChangesAsync();
    }
}
