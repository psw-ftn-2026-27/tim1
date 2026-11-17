using Payment.Application.ProductCatalog;
using Payment.Domain.Products;

namespace Payment.Application.Products;

public interface IProductReadRepository
{
    Task<List<ProductDto>> GetProductsAsync();
    Task<List<ProductDto>> SearchProductsAsync(decimal? minPrice, decimal? maxPrice, ProductType? type);
}
