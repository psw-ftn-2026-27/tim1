using Payment.Domain.Wallets;

namespace Payment.Application.Wallets;

public interface IWalletRepository
{
    Task<Wallet?> GetByOwnerAsync(Guid ownerId);
    void Add(Wallet wallet);
}
