using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Purchases;

/// <summary>
/// Puts a product in a cart, unless its owner already bought it. The cart alone cannot tell,
/// because it never sees a purchase, so the rule lives here and not in the application service.
/// </summary>
public sealed class CartAddition
{
    public void Add(ShoppingCart cart, Product product, IReadOnlyList<Purchase> ownedPurchases)
    {
        if (ownedPurchases.Any(purchase => purchase.Reference == product.Reference))
        {
            throw new DomainException($"You already own {product.Name}.");
        }

        cart.AddItem(product.Reference, product.Name, product.Price);
    }
}
