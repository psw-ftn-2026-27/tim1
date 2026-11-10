using Payment.Application.WalletFunding;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<WalletOverviewDto> GetOverviewAsync(Guid ownerId);
}
