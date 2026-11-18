using Payment.Domain.Products;

namespace Payment.Application.ProductCatalog;

public sealed record RegisterProductDto(
    ProductType Type,
    Guid EntityId,
    string Name,
    decimal Price,
    Guid OwnerId);
