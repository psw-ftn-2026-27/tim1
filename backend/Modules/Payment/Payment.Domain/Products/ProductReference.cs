using Shared.Domain.Exceptions;

namespace Payment.Domain.Products;

/// <summary>
/// Which entity of which module a product stands for. The pair identifies the product from
/// the registering module's point of view, and the two values only mean anything together.
/// </summary>
public sealed record ProductReference
{
    public ProductType EntityType { get; private init; }
    public Guid EntityId { get; private init; }

    private ProductReference() { }

    public ProductReference(ProductType entityType, Guid entityId)
    {
        if (entityId == Guid.Empty)
        {
            throw new DomainException("A product reference requires the ID of the entity it stands for.");
        }

        EntityType = entityType;
        EntityId = entityId;
    }
}
