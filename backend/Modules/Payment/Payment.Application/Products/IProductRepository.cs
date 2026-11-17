using Payment.Domain.Products;

namespace Payment.Application.Products;

public interface IProductRepository
{
    Task<Product?> GetByReferenceAsync(ProductReference reference);
    void Add(Product product);
}
