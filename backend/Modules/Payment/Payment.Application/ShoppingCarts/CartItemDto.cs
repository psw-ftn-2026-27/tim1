using Payment.Domain.Products;

namespace Payment.Application.ShoppingCarts;

public sealed record CartItemDto(
    ProductType Type,
    Guid EntityId,
    string Name,
    decimal Price);
