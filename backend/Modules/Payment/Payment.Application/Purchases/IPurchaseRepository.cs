using Payment.Domain.Products;
using Payment.Domain.Purchases;

namespace Payment.Application.Purchases;

public interface IPurchaseRepository
{
    Task<List<Purchase>> GetOfBuyerAsync(Guid buyerId, IReadOnlyList<ProductReference> references);
    void Add(Purchase purchase);
}
