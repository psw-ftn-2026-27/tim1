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

    /// <summary>
    /// The products the given references stand for. The database narrows the rows down by entity
    /// ID, and the pair of kind and ID decides which of them answer a reference.
    /// </summary>
    public async Task<List<Product>> GetByReferencesAsync(IReadOnlyList<ProductReference> references)
    {
        var entityIds = references.Select(reference => reference.EntityId).ToList();

        var products = await _dbContext.Products
            .Where(product => entityIds.Contains(product.Reference.EntityId))
            .ToListAsync();

        return [.. products.Where(product => references.Contains(product.Reference))];
    }

    public Task<List<Product>> GetByOwnerAsync(Guid ownerId)
    {
        return _dbContext.Products.Where(product => product.OwnerId == ownerId).ToListAsync();
    }

    public void Add(Product product)
    {
        _dbContext.Products.Add(product);
    }
}
