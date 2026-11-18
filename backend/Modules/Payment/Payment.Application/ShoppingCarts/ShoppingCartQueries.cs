namespace Payment.Application.ShoppingCarts;

public sealed class ShoppingCartQueries
{
    private readonly IShoppingCartReadRepository _cartReadRepository;
    private readonly IShoppingCartRepository _cartRepository;

    public ShoppingCartQueries(
        IShoppingCartReadRepository cartReadRepository,
        IShoppingCartRepository cartRepository)
    {
        _cartReadRepository = cartReadRepository;
        _cartRepository = cartRepository;
    }

    /// <summary>
    /// Reads the cart through the aggregate, because the total is a value the cart derives from
    /// its own items; deriving it a second time in SQL would put the rule in two places. A
    /// customer without a cart sees an empty one, and nothing is written.
    /// </summary>
    public async Task<CartDto> GetCartAsync(Guid ownerId)
    {
        var cart = await _cartRepository.GetByOwnerAsync(ownerId);
        if (cart is null)
        {
            return new CartDto([], 0);
        }

        var items = cart.Items
            .Select(item => new CartItemDto(
                item.Reference.EntityType,
                item.Reference.EntityId,
                item.Name,
                item.Price))
            .ToList();

        return new CartDto(items, cart.Total());
    }

    public Task<int> GetCartItemCountAsync(Guid ownerId)
    {
        return _cartReadRepository.GetItemCountAsync(ownerId);
    }
}
