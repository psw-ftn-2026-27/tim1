using Payment.Application.ProductCatalog;

namespace Payment.Application.Products;

public interface IProductReadRepository
{
    Task<List<ProductDto>> GetProductsAsync();
}
