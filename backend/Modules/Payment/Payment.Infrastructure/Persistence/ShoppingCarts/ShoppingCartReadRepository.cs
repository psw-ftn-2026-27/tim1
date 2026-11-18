using Microsoft.EntityFrameworkCore;
using Payment.Application.Products;
using Payment.Application.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal sealed class ShoppingCartReadRepository : IShoppingCartReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public ShoppingCartReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ProductReferenceDto>> GetItemReferencesAsync(Guid ownerId)
    {
        return _dbContext.ShoppingCarts
            .Where(cart => cart.OwnerId == ownerId)
            .SelectMany(cart => cart.Items)
            .Select(item => new ProductReferenceDto(item.Reference.EntityType, item.Reference.EntityId))
            .ToListAsync();
    }

    public Task<int> GetItemCountAsync(Guid ownerId)
    {
        return _dbContext.ShoppingCarts
            .Where(cart => cart.OwnerId == ownerId)
            .Select(cart => cart.Items.Count)
            .FirstOrDefaultAsync();
    }
}
