using Payment.Application.TopUpRequests;
using Payment.Application.Wallets;
using Payment.Application.WithdrawalRequests;
using Payment.Domain.TopUpRequests;
using Payment.Domain.Wallets;
using Payment.Domain.WithdrawalRequests;
using Shared.Domain.Exceptions;

namespace Payment.Application.WalletRequestReview;

public sealed class WalletRequestReviewService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITopUpRequestRepository _topUpRequestRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletRequestReviewService(
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

    public async Task ApproveTopUpAsync(Guid requestId)
    {
        var request = await GetTopUpRequestAsync(requestId);
        var wallet = await GetWalletAsync(request.WalletId);

        request.Approve();
        wallet.Deposit(request.Amount);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectTopUpAsync(Guid requestId, RejectRequestDto dto)
    {
        var request = await GetTopUpRequestAsync(requestId);

        request.Reject(dto.Reason);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ApproveWithdrawalAsync(Guid requestId)
    {
        var request = await GetWithdrawalRequestAsync(requestId);
        var wallet = await GetWalletAsync(request.WalletId);

        request.Approve();
        wallet.PayOutReserved(request.Amount);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectWithdrawalAsync(Guid requestId, RejectRequestDto dto)
    {
        var request = await GetWithdrawalRequestAsync(requestId);
        var wallet = await GetWalletAsync(request.WalletId);

        request.Reject(dto.Reason);
        wallet.ReturnReserved(request.Amount);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<TopUpRequest> GetTopUpRequestAsync(Guid requestId)
    {
        var request = await _topUpRequestRepository.GetByIdAsync(requestId);
        if (request is null)
        {
            throw new NotFoundException($"Top-up request {requestId} does not exist.");
        }

        return request;
    }

    private async Task<WithdrawalRequest> GetWithdrawalRequestAsync(Guid requestId)
    {
        var request = await _withdrawalRequestRepository.GetByIdAsync(requestId);
        if (request is null)
        {
            throw new NotFoundException($"Withdrawal request {requestId} does not exist.");
        }

        return request;
    }

    private async Task<Wallet> GetWalletAsync(Guid walletId)
    {
        var wallet = await _walletRepository.GetByIdAsync(walletId);
        if (wallet is null)
        {
            throw new NotFoundException($"Wallet {walletId} does not exist.");
        }

        return wallet;
    }
}
