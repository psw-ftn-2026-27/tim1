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

    /// <summary>
    /// Holds both tours at the price their products carry, for a customer who owns neither of
    /// them and whose wallet covers the whole cart.
    /// </summary>
    public static readonly ShoppingCart BuyersCart;

    /// <summary>Holds a tour no product stands for any more.</summary>
    public static readonly ShoppingCart CartWithAnUnregisteredProduct;

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

        BuyersCart = new ShoppingCart(DemoUsers.SecondExplorer);
        BuyersCart.AddItem(
            new ProductReference(ProductType.Tour, ProductSeed.DemoExplorersTour.Reference.EntityId),
            ProductSeed.DemoExplorersTour.Name,
            ProductSeed.DemoExplorersTour.Price);
        BuyersCart.AddItem(
            new ProductReference(ProductType.Tour, ProductSeed.TourOfAForgottenOwner.Reference.EntityId),
            ProductSeed.TourOfAForgottenOwner.Name,
            ProductSeed.TourOfAForgottenOwner.Price);

        CartWithAnUnregisteredProduct = new ShoppingCart(DemoUsers.Explorer);
        CartWithAnUnregisteredProduct.AddItem(
            new ProductReference(ProductType.Tour, Guid.Parse("30000000-0000-0000-0000-000000000009")),
            "Tour of the city museums",
            700m);
    }

    public static ShoppingCart[] All =>
        [ExplorersCart, SecondExplorersCart, BuyersCart, CartWithAnUnregisteredProduct];
}
