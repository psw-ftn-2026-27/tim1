using Microsoft.EntityFrameworkCore;
using Payment.Application.WalletFunding;
using Payment.Application.WalletRequestReview;
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
        var wallet = await ReadWalletAsync(ownerId);
        if (wallet is null)
        {
            return new WalletOverviewDto(0, 0, [], []);
        }

        var topUps = await ReadOwnTopUpsAsync(wallet.Id);
        var withdrawals = await ReadOwnWithdrawalsAsync(wallet.Id);

        return new WalletOverviewDto(
            wallet.AvailableBalance,
            wallet.ReservedBalance,
            wallet.Transactions,
            NewestFirst([.. topUps, .. withdrawals], request => request.CreatedAt));
    }

    public async Task<WalletRequestsDto> GetRequestsAsync()
    {
        var topUps = await ReadTopUpReviewsAsync();
        var withdrawals = await ReadWithdrawalReviewsAsync();
        List<WalletRequestReviewDto> requests = [.. topUps, .. withdrawals];

        return new WalletRequestsDto(
            NewestFirst(WithStatus(requests, pending: true), request => request.CreatedAt),
            NewestFirst(WithStatus(requests, pending: false), request => request.CreatedAt));
    }

    private Task<WalletSnapshot?> ReadWalletAsync(Guid ownerId)
    {
        return _dbContext.Wallets
            .Where(wallet => wallet.OwnerId == ownerId)
            .Select(wallet => new WalletSnapshot(
                wallet.Id,
                wallet.AvailableBalance,
                wallet.ReservedBalance,
                wallet.Transactions
                    .OrderByDescending(transaction => transaction.OccurredAt)
                    .Select(transaction => new WalletTransactionDto(
                        transaction.OccurredAt,
                        transaction.Type,
                        transaction.Amount,
                        transaction.BalanceAfter))
                    .ToList()))
            .FirstOrDefaultAsync();
    }

    private async Task<List<WalletRequestDto>> ReadOwnTopUpsAsync(Guid walletId)
    {
        var rows = await _dbContext.TopUpRequests
            .Where(topUpRequest => topUpRequest.WalletId == walletId)
            .Select(topUpRequest => new
            {
                topUpRequest.Id,
                topUpRequest.Amount,
                topUpRequest.CreatedAt,
                topUpRequest.Status,
                topUpRequest.RejectionReason
            })
            .ToListAsync();

        return [.. rows.Select(row => new WalletRequestDto(
            row.Id,
            WalletRequestType.TopUp,
            row.Amount,
            ToWalletRequestStatus(row.Status),
            row.CreatedAt,
            row.RejectionReason))];
    }

    private async Task<List<WalletRequestDto>> ReadOwnWithdrawalsAsync(Guid walletId)
    {
        var rows = await _dbContext.WithdrawalRequests
            .Where(withdrawalRequest => withdrawalRequest.WalletId == walletId)
            .Select(withdrawalRequest => new
            {
                withdrawalRequest.Id,
                withdrawalRequest.Amount,
                withdrawalRequest.CreatedAt,
                withdrawalRequest.Status,
                withdrawalRequest.RejectionReason
            })
            .ToListAsync();

        return [.. rows.Select(row => new WalletRequestDto(
            row.Id,
            WalletRequestType.Withdrawal,
            row.Amount,
            ToWalletRequestStatus(row.Status),
            row.CreatedAt,
            row.RejectionReason))];
    }

    private async Task<List<WalletRequestReviewDto>> ReadTopUpReviewsAsync()
    {
        var rows = await _dbContext.TopUpRequests
            .Join(
                _dbContext.Wallets,
                topUpRequest => topUpRequest.WalletId,
                wallet => wallet.Id,
                (topUpRequest, wallet) => new
                {
                    topUpRequest.Id,
                    wallet.OwnerId,
                    topUpRequest.Amount,
                    topUpRequest.CreatedAt,
                    topUpRequest.Status,
                    topUpRequest.RejectionReason
                })
            .ToListAsync();

        return [.. rows.Select(row => new WalletRequestReviewDto(
            row.Id,
            WalletRequestType.TopUp,
            row.OwnerId,
            row.Amount,
            row.CreatedAt,
            ToWalletRequestStatus(row.Status),
            row.RejectionReason))];
    }

    private async Task<List<WalletRequestReviewDto>> ReadWithdrawalReviewsAsync()
    {
        var rows = await _dbContext.WithdrawalRequests
            .Join(
                _dbContext.Wallets,
                withdrawalRequest => withdrawalRequest.WalletId,
                wallet => wallet.Id,
                (withdrawalRequest, wallet) => new
                {
                    withdrawalRequest.Id,
                    wallet.OwnerId,
                    withdrawalRequest.Amount,
                    withdrawalRequest.CreatedAt,
                    withdrawalRequest.Status,
                    withdrawalRequest.RejectionReason
                })
            .ToListAsync();

        return [.. rows.Select(row => new WalletRequestReviewDto(
            row.Id,
            WalletRequestType.Withdrawal,
            row.OwnerId,
            row.Amount,
            row.CreatedAt,
            ToWalletRequestStatus(row.Status),
            row.RejectionReason))];
    }

    private static List<WalletRequestReviewDto> WithStatus(
        List<WalletRequestReviewDto> requests,
        bool pending)
    {
        return [.. requests.Where(request =>
            (request.Status == WalletRequestStatus.Pending) == pending)];
    }

    private static List<T> NewestFirst<T>(List<T> items, Func<T, DateTime> createdAt)
    {
        return [.. items.OrderByDescending(createdAt)];
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

    private sealed record WalletSnapshot(
        Guid Id,
        decimal AvailableBalance,
        decimal ReservedBalance,
        List<WalletTransactionDto> Transactions);
}
