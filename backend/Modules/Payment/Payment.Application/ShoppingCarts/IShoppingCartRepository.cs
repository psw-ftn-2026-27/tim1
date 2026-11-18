using Payment.Domain.ShoppingCarts;

namespace Payment.Application.ShoppingCarts;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetByOwnerAsync(Guid ownerId);
    void Add(ShoppingCart cart);
}
