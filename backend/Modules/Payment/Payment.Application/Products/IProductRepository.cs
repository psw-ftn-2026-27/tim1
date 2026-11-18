using Payment.Domain.Products;

namespace Payment.Application.Products;

public interface IProductRepository
{
    Task<Product?> GetByReferenceAsync(ProductReference reference);
    Task<List<Product>> GetByReferencesAsync(IReadOnlyList<ProductReference> references);
    Task<List<Product>> GetByOwnerAsync(Guid ownerId);
    void Add(Product product);
}
