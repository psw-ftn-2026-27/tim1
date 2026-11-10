using Payment.Application.WalletFunding;
using Payment.Application.WalletRequestReview;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<WalletOverviewDto> GetOverviewAsync(Guid ownerId);
    Task<WalletRequestsDto> GetRequestsAsync();
}
