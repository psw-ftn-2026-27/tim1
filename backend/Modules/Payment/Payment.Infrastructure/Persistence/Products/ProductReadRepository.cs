using Microsoft.EntityFrameworkCore;
using Payment.Application.ProductCatalog;
using Payment.Application.Products;

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
        return _dbContext.Products
            .OrderBy(product => product.Name)
            .Select(product => new ProductDto(
                product.Id,
                product.Reference.EntityType,
                product.Name,
                product.Price,
                product.OwnerId,
                ""))
            .ToListAsync();
    }
}
