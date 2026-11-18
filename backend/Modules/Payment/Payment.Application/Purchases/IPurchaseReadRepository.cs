using Payment.Application.Products;

namespace Payment.Application.Purchases;

public interface IPurchaseReadRepository
{
    Task<List<ProductReferenceDto>> GetPurchasedReferencesAsync(Guid buyerId);
}
