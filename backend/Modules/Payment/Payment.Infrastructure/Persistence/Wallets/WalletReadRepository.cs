using Microsoft.EntityFrameworkCore;
using Payment.Application.WalletFunding;
using Payment.Application.Wallets;
using Payment.Domain.TopUpRequests;
using Payment.Domain.WithdrawalRequests;

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
        var overview = await _dbContext.Wallets
            .Where(wallet => wallet.OwnerId == ownerId)
            .Select(wallet => new
            {
                wallet.AvailableBalance,
                wallet.ReservedBalance,
                Transactions = wallet.Transactions
                    .OrderByDescending(transaction => transaction.OccurredAt)
                    .Select(transaction => new WalletTransactionDto(
                        transaction.OccurredAt,
                        transaction.Type,
                        transaction.Amount,
                        transaction.BalanceAfter))
                    .ToList(),
                TopUpRequests = _dbContext.TopUpRequests
                    .Where(topUpRequest => topUpRequest.WalletId == wallet.Id)
                    .Select(topUpRequest => new
                    {
                        topUpRequest.Id,
                        topUpRequest.Amount,
                        topUpRequest.Status,
                        topUpRequest.CreatedAt
                    })
                    .ToList(),
                WithdrawalRequests = _dbContext.WithdrawalRequests
                    .Where(withdrawalRequest => withdrawalRequest.WalletId == wallet.Id)
                    .Select(withdrawalRequest => new
                    {
                        withdrawalRequest.Id,
                        withdrawalRequest.Amount,
                        withdrawalRequest.Status,
                        withdrawalRequest.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (overview is null)
        {
            return new WalletOverviewDto(0, 0, [], []);
        }

        var topUps = overview.TopUpRequests
            .Select(topUpRequest => new WalletRequestDto(
                topUpRequest.Id,
                WalletRequestType.TopUp,
                topUpRequest.Amount,
                ToWalletRequestStatus(topUpRequest.Status),
                topUpRequest.CreatedAt));

        var withdrawals = overview.WithdrawalRequests
            .Select(withdrawalRequest => new WalletRequestDto(
                withdrawalRequest.Id,
                WalletRequestType.Withdrawal,
                withdrawalRequest.Amount,
                ToWalletRequestStatus(withdrawalRequest.Status),
                withdrawalRequest.CreatedAt));

        return new WalletOverviewDto(
            overview.AvailableBalance,
            overview.ReservedBalance,
            overview.Transactions,
            [.. topUps.Concat(withdrawals).OrderByDescending(request => request.CreatedAt)]);
    }

    private static WalletRequestStatus ToWalletRequestStatus(TopUpRequestStatus status) => status switch
    {
        TopUpRequestStatus.Pending => WalletRequestStatus.Pending,
        TopUpRequestStatus.Approved => WalletRequestStatus.Approved,
        _ => WalletRequestStatus.Rejected
    };

    private static WalletRequestStatus ToWalletRequestStatus(WithdrawalRequestStatus status) => status switch
    {
        WithdrawalRequestStatus.Pending => WalletRequestStatus.Pending,
        WithdrawalRequestStatus.Approved => WalletRequestStatus.Approved,
        _ => WalletRequestStatus.Rejected
    };
}
