using Payment.Application.Products;
using Payment.Domain.Products;

namespace Payment.Application.Purchases;

public interface IPurchaseReadRepository
{
    Task<List<ProductReferenceDto>> GetPurchasedReferencesAsync(Guid buyerId);
    Task<bool> HasPurchasedAsync(Guid buyerId, ProductReference reference);
    Task<bool> HasAnyPurchaseAsync(ProductReference reference);
}
