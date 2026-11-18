using Microsoft.EntityFrameworkCore;
using Payment.Application.ShoppingCarts;
using Payment.Domain.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal sealed class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly PaymentDbContext _dbContext;

    public ShoppingCartRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ShoppingCart?> GetByOwnerAsync(Guid ownerId)
    {
        return _dbContext.ShoppingCarts.FirstOrDefaultAsync(cart => cart.OwnerId == ownerId);
    }

    public void Add(ShoppingCart cart)
    {
        _dbContext.ShoppingCarts.Add(cart);
    }
}
