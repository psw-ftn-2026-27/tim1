using Payment.Application.Purchases;
using Payment.Domain.Products;

namespace Payment.Application.PurchaseVerification;

/// <summary>
/// Answers whether a user owns a product. The module that owns the content asks this through
/// Payment's contract, because a purchase is Payment's own record.
/// </summary>
public sealed class PurchaseVerificationQueries
{
    private readonly IPurchaseReadRepository _purchaseReadRepository;

    public PurchaseVerificationQueries(IPurchaseReadRepository purchaseReadRepository)
    {
        _purchaseReadRepository = purchaseReadRepository;
    }

    public Task<bool> HasPurchasedAsync(Guid userId, ProductReference reference)
    {
        return _purchaseReadRepository.HasPurchasedAsync(userId, reference);
    }
}
