using Payment.Application.Products;

namespace Payment.Application.ShoppingCarts;

public interface IShoppingCartReadRepository
{
    Task<int> GetItemCountAsync(Guid ownerId);
    Task<List<ProductReferenceDto>> GetItemReferencesAsync(Guid ownerId);
}
