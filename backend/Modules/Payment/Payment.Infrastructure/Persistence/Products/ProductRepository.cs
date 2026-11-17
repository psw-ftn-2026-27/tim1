using Microsoft.EntityFrameworkCore;
using Payment.Application.Products;
using Payment.Domain.Products;

namespace Payment.Infrastructure.Persistence.Products;

internal sealed class ProductRepository : IProductRepository
{
    private readonly PaymentDbContext _dbContext;

    public ProductRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Product?> GetByReferenceAsync(ProductReference reference)
    {
        return _dbContext.Products.FirstOrDefaultAsync(product =>
            product.Reference.EntityType == reference.EntityType
            && product.Reference.EntityId == reference.EntityId);
    }

    public void Add(Product product)
    {
        _dbContext.Products.Add(product);
    }
}
