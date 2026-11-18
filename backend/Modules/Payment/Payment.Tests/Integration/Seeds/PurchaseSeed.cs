using Payment.Domain.Products;
using Payment.Domain.Purchases;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

/// <summary>
/// Every purchase builds a <see cref="ProductReference"/> of its own instead of reusing the one
/// the product seed holds: a reference is an owned value, and one instance belongs to one owner.
/// </summary>
internal static class PurchaseSeed
{
    /// <summary>
    /// What the second explorer has already bought. It is the tour their cart still holds, so a
    /// checkout of that cart is refused.
    /// </summary>
    public static readonly Purchase SecondExplorersRide = new(
        WellKnownUsers.SecondExplorer,
        new ProductReference(ProductType.Tour, ProductSeed.TourOfAForgottenOwner.Reference.EntityId),
        ProductSeed.TourOfAForgottenOwner.Price);

    public static Purchase[] All => [SecondExplorersRide];
}
