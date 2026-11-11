using Microsoft.EntityFrameworkCore;
using Payment.Application.WalletFunding;
using Payment.Application.WalletRequestReview;
using Payment.Application.Wallets;
using Payment.Domain.Requests;
using Payment.Domain.TopUpRequests;
using Payment.Domain.Wallets;
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
        var wallet = await ProjectToDtos(_dbContext.Wallets.Where(wallet => wallet.OwnerId == ownerId))
            .FirstOrDefaultAsync();

        if (wallet is null)
        {
            return new WalletOverviewDto(0, 0, [], []);
        }

        var topUps = await ProjectToDtos(TopUpsOf(ownerId)).ToListAsync();
        var withdrawals = await ProjectToDtos(WithdrawalsOf(ownerId)).ToListAsync();

        return wallet with
        {
            Requests = [.. topUps.Concat(withdrawals).OrderByDescending(request => request.CreatedAt)]
        };
    }

    public async Task<WalletRequestsDto> GetRequestsAsync()
    {
        var topUps = await ProjectToReviewDtos(_dbContext.TopUpRequests).ToListAsync();
        var withdrawals = await ProjectToReviewDtos(_dbContext.WithdrawalRequests).ToListAsync();
        var requests = topUps.Concat(withdrawals).OrderByDescending(request => request.CreatedAt);

        return new WalletRequestsDto(
            [.. requests.Where(request => request.Status == WalletRequestStatus.Pending)],
            [.. requests.Where(request => request.Status != WalletRequestStatus.Pending)]);
    }

    private IQueryable<TopUpRequest> TopUpsOf(Guid ownerId)
    {
        return _dbContext.TopUpRequests.Where(request =>
            _dbContext.Wallets.Any(wallet => wallet.Id == request.WalletId && wallet.OwnerId == ownerId));
    }

    private IQueryable<WithdrawalRequest> WithdrawalsOf(Guid ownerId)
    {
        return _dbContext.WithdrawalRequests.Where(request =>
            _dbContext.Wallets.Any(wallet => wallet.Id == request.WalletId && wallet.OwnerId == ownerId));
    }

    private static IQueryable<WalletOverviewDto> ProjectToDtos(IQueryable<Wallet> wallets)
    {
        return wallets
            .Select(wallet => new WalletOverviewDto(
                wallet.AvailableBalance,
                wallet.ReservedBalance,
                wallet.Transactions
                    .OrderByDescending(transaction => transaction.OccurredAt)
                    .Select(transaction => new WalletTransactionDto(
                        transaction.OccurredAt, transaction.Type, transaction.Amount, transaction.BalanceAfter))
                    .ToList(),
                new List<WalletRequestDto>()));
    }

    private static IQueryable<WalletRequestDto> ProjectToDtos(IQueryable<TopUpRequest> requests)
    {
        return requests
            .Select(request => new WalletRequestDto(
                request.Id,
                WalletRequestType.TopUp,
                request.Amount,
                request.Status,
                request.CreatedAt,
                request.RejectionReason));
    }

    private static IQueryable<WalletRequestDto> ProjectToDtos(IQueryable<WithdrawalRequest> requests)
    {
        return requests
            .Select(request => new WalletRequestDto(
                request.Id,
                WalletRequestType.Withdrawal,
                request.Amount,
                request.Status,
                request.CreatedAt,
                request.RejectionReason));
    }

    private IQueryable<WalletRequestReviewDto> ProjectToReviewDtos(IQueryable<TopUpRequest> requests)
    {
        return requests.Join(
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
                request.RejectionReason));
    }

    private IQueryable<WalletRequestReviewDto> ProjectToReviewDtos(IQueryable<WithdrawalRequest> requests)
    {
        return requests.Join(
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
                request.RejectionReason));
    }
}
