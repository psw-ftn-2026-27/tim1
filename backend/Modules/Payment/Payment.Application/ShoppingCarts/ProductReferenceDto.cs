using Payment.Domain.Products;

namespace Payment.Application.ShoppingCarts;

/// <summary>Which product a cart operation is about, as the client names it.</summary>
public sealed record ProductReferenceDto(
    ProductType Type,
    Guid EntityId);
