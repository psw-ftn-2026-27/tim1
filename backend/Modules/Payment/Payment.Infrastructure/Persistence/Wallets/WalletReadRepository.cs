using Microsoft.EntityFrameworkCore;
using Payment.Application.WalletFunding;
using Payment.Application.Wallets;

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
            .Select(wallet => new WalletOverviewDto(
                wallet.AvailableBalance,
                wallet.ReservedBalance,
                wallet.Transactions
                    .OrderByDescending(transaction => transaction.OccurredAt)
                    .Select(transaction => new WalletTransactionDto(
                        transaction.OccurredAt,
                        transaction.Type,
                        transaction.Amount,
                        transaction.BalanceAfter))
                    .ToList(),
                _dbContext.TopUpRequests
                    .Where(topUpRequest => topUpRequest.WalletId == wallet.Id)
                    .OrderByDescending(topUpRequest => topUpRequest.CreatedAt)
                    .Select(topUpRequest => new WalletRequestDto(
                        topUpRequest.Id,
                        WalletRequestType.TopUp,
                        topUpRequest.Amount,
                        topUpRequest.Status,
                        topUpRequest.CreatedAt))
                    .ToList()))
            .FirstOrDefaultAsync();

        return overview ?? new WalletOverviewDto(0, 0, [], []);
    }
}
