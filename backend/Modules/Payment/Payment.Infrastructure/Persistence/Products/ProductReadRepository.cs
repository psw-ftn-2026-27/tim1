using Microsoft.EntityFrameworkCore;
using Payment.Application.ProductCatalog;
using Payment.Application.Products;
using Payment.Domain.Products;

namespace Payment.Infrastructure.Persistence.Products;

internal sealed class ProductReadRepository : IProductReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public ProductReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ProductDto>> GetProductsAsync()
    {
        return SearchProductsAsync(null, null, null);
    }

    public Task<List<ProductDto>> SearchProductsAsync(decimal? minPrice, decimal? maxPrice, ProductType? type)
    {
        return _dbContext.Products
            .Where(product =>
                (minPrice == null || product.Price >= minPrice)
                && (maxPrice == null || product.Price <= maxPrice)
                && (type == null || product.Reference.EntityType == type))
            .OrderBy(product => product.Name)
            .Select(product => new ProductDto(
                product.Id,
                product.Reference.EntityType,
                product.Reference.EntityId,
                product.Name,
                product.Price,
                product.OwnerId,
                "",
                false,
                false))
            .ToListAsync();
    }
}
