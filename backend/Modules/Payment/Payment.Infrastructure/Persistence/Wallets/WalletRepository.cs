using Microsoft.EntityFrameworkCore;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;

namespace Payment.Infrastructure.Persistence.Wallets;

internal sealed class WalletRepository : IWalletRepository
{
    private readonly PaymentDbContext _dbContext;

    public WalletRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Wallet?> GetByIdAsync(Guid id)
    {
        return _dbContext.Wallets.FirstOrDefaultAsync(wallet => wallet.Id == id);
    }

    public Task<Wallet?> GetByOwnerAsync(Guid ownerId)
    {
        return _dbContext.Wallets.FirstOrDefaultAsync(wallet => wallet.OwnerId == ownerId);
    }

    public void Add(Wallet wallet)
    {
        _dbContext.Wallets.Add(wallet);
    }
}
