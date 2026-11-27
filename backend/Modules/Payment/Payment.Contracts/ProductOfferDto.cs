namespace Payment.Contracts;

/// <summary>
/// A product as the platform offers it: the product's own ID, which identifies it in every
/// later Payment operation, the kind of content it stands for, the ID the owning module gave
/// the entity, and what a buyer sees about it. EntityId is how the owning module recognises
/// its own content in the offer. OwnerName is the display name Identity knows for the owner,
/// or a stand-in text when Identity no longer knows the owner.
/// </summary>
public sealed record ProductOfferDto(
    Guid Id,
    ProductType Type,
    Guid EntityId,
    string Name,
    decimal Price,
    Guid OwnerId,
    string OwnerName);
