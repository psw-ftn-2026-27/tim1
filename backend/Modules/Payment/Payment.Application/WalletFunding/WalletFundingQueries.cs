using Payment.Application.Wallets;

namespace Payment.Application.WalletFunding;

public sealed class WalletFundingQueries
{
    private readonly IWalletReadRepository _walletReadRepository;

    public WalletFundingQueries(IWalletReadRepository walletReadRepository)
    {
        _walletReadRepository = walletReadRepository;
    }

    public Task<WalletOverviewDto> GetOverviewAsync(Guid ownerId)
    {
        return _walletReadRepository.GetOverviewAsync(ownerId);
    }
}
