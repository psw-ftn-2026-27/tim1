using Payment.Domain.Products;

namespace Payment.Application.Products;

/// <summary>Which product an operation is about, as the client names it.</summary>
public sealed record ProductReferenceDto(
    ProductType Type,
    Guid EntityId);
