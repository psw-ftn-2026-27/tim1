using Payment.Application.Wallets;

namespace Payment.Application.WalletRequestReview;

public sealed class WalletRequestReviewQueries
{
    private readonly IWalletReadRepository _walletReadRepository;

    public WalletRequestReviewQueries(IWalletReadRepository walletReadRepository)
    {
        _walletReadRepository = walletReadRepository;
    }

    public Task<WalletRequestsDto> GetRequestsAsync()
    {
        return _walletReadRepository.GetRequestsAsync();
    }
}
