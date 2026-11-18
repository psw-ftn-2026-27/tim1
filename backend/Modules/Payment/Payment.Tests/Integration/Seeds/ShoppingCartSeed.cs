using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

/// <summary>
/// Every item builds a <see cref="ProductReference"/> of its own instead of reusing the one the
/// product seed holds: a reference is an owned value, and one instance belongs to one owner.
/// </summary>
internal static class ShoppingCartSeed
{
    /// <summary>
    /// Holds both tours. The fortress tour was added while it still cost 1000, so the item
    /// carries less than the product costs today.
    /// </summary>
    public static readonly ShoppingCart ExplorersCart;

    /// <summary>Holds the tour of a forgotten owner alone.</summary>
    public static readonly ShoppingCart SecondExplorersCart;

    static ShoppingCartSeed()
    {
        ExplorersCart = new ShoppingCart(WellKnownUsers.Explorer);
        ExplorersCart.AddItem(
            new ProductReference(ProductType.Tour, ProductSeed.DemoExplorersTour.Reference.EntityId),
            ProductSeed.DemoExplorersTour.Name,
            1000m);
        ExplorersCart.AddItem(
            new ProductReference(ProductType.Tour, ProductSeed.TourOfAForgottenOwner.Reference.EntityId),
            ProductSeed.TourOfAForgottenOwner.Name,
            ProductSeed.TourOfAForgottenOwner.Price);

        SecondExplorersCart = new ShoppingCart(WellKnownUsers.SecondExplorer);
        SecondExplorersCart.AddItem(
            new ProductReference(ProductType.Tour, ProductSeed.TourOfAForgottenOwner.Reference.EntityId),
            ProductSeed.TourOfAForgottenOwner.Name,
            ProductSeed.TourOfAForgottenOwner.Price);
    }

    public static ShoppingCart[] All => [ExplorersCart, SecondExplorersCart];
}
