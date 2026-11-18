namespace Payment.Application.ShoppingCarts;

public interface IShoppingCartReadRepository
{
    Task<int> GetItemCountAsync(Guid ownerId);
}
