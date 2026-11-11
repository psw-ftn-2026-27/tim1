using Microsoft.EntityFrameworkCore;
using Payment.Application.WalletFunding;
using Payment.Application.WalletRequestReview;
using Payment.Application.Wallets;
using Payment.Domain.Requests;

namespace Payment.Infrastructure.Persistence.Wallets;

internal sealed class WalletReadRepository : IWalletReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public WalletReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WalletOverviewDto> GetOverviewAsync(Guid ownerId)
    {
        var wallet = await _dbContext.Wallets
            .Where(wallet => wallet.OwnerId == ownerId)
            .Select(wallet => new
            {
                wallet.Id,
                wallet.AvailableBalance,
                wallet.ReservedBalance,
                Transactions = wallet.Transactions
                    .OrderByDescending(transaction => transaction.OccurredAt)
                    .Select(transaction => new WalletTransactionDto(
                        transaction.OccurredAt,
                        transaction.Type,
                        transaction.Amount,
                        transaction.BalanceAfter))
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (wallet is null)
        {
            return new WalletOverviewDto(0, 0, [], []);
        }

        var topUps = await _dbContext.TopUpRequests
            .Where(request => request.WalletId == wallet.Id)
            .Select(request => new WalletRequestDto(
                request.Id,
                WalletRequestType.TopUp,
                request.Amount,
                request.Status,
                request.CreatedAt,
                request.RejectionReason))
            .ToListAsync();

        var withdrawals = await _dbContext.WithdrawalRequests
            .Where(request => request.WalletId == wallet.Id)
            .Select(request => new WalletRequestDto(
                request.Id,
                WalletRequestType.Withdrawal,
                request.Amount,
                request.Status,
                request.CreatedAt,
                request.RejectionReason))
            .ToListAsync();

        return new WalletOverviewDto(
            wallet.AvailableBalance,
            wallet.ReservedBalance,
            wallet.Transactions,
            [.. topUps.Concat(withdrawals).OrderByDescending(request => request.CreatedAt)]);
    }

    public async Task<WalletRequestsDto> GetRequestsAsync()
    {
        var topUps = await _dbContext.TopUpRequests
            .Join(
                _dbContext.Wallets,
                request => request.WalletId,
                wallet => wallet.Id,
                (request, wallet) => new WalletRequestReviewDto(
                    request.Id,
                    WalletRequestType.TopUp,
                    wallet.OwnerId,
                    request.Amount,
                    request.CreatedAt,
                    request.Status,
                    request.RejectionReason))
            .ToListAsync();

        var withdrawals = await _dbContext.WithdrawalRequests
            .Join(
                _dbContext.Wallets,
                request => request.WalletId,
                wallet => wallet.Id,
                (request, wallet) => new WalletRequestReviewDto(
                    request.Id,
                    WalletRequestType.Withdrawal,
                    wallet.OwnerId,
                    request.Amount,
                    request.CreatedAt,
                    request.Status,
                    request.RejectionReason))
            .ToListAsync();

        var requests = topUps.Concat(withdrawals).OrderByDescending(request => request.CreatedAt);

        return new WalletRequestsDto(
            [.. requests.Where(request => request.Status == WalletRequestStatus.Pending)],
            [.. requests.Where(request => request.Status != WalletRequestStatus.Pending)]);
    }
}
