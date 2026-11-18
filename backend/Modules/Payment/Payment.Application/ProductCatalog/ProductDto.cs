using Payment.Domain.Products;

namespace Payment.Application.ProductCatalog;

public sealed record ProductDto(
    Guid Id,
    ProductType Type,
    Guid EntityId,
    string Name,
    decimal Price,
    Guid OwnerId,
    string OwnerName);
