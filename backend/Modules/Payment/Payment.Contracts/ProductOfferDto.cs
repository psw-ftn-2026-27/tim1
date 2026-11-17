namespace Payment.Contracts;

/// <summary>
/// A product as the platform offers it: the product's own ID, which identifies it in every
/// later Payment operation, the kind of content it stands for, and what a buyer sees about
/// it. OwnerName is the display name Identity knows for the owner, or a stand-in text when
/// Identity no longer knows the owner.
/// </summary>
public sealed record ProductOfferDto(
    Guid Id,
    ProductType Type,
    string Name,
    decimal Price,
    Guid OwnerId,
    string OwnerName);
