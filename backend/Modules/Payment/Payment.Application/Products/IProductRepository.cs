using Payment.Domain.Products;

namespace Payment.Application.Products;

public interface IProductRepository
{
    Task<Product?> GetByReferenceAsync(ProductReference reference);
    Task<List<Product>> GetByReferencesAsync(IReadOnlyList<ProductReference> references);
    void Add(Product product);
}
