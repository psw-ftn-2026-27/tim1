using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Purchases;

/// <summary>
/// Buys a whole cart at once: it refuses a cart holding something its owner already bought,
/// moves the price of every item from the customer's wallet to the wallet of the item's owner,
/// leaves one purchase behind for each item and empties the cart. The purchases are handed back
/// so the command that saves can store them.
/// </summary>
public sealed class CartCheckout
{
    public IReadOnlyList<Purchase> Checkout(
        ShoppingCart cart,
        IReadOnlyList<Product> products,
        Wallet buyerWallet,
        IReadOnlyDictionary<Guid, Wallet> sellerWallets,
        IReadOnlyList<Purchase> ownedPurchases)
    {
        if (cart.Items.Count == 0)
        {
            throw new DomainException("An empty cart cannot be bought.");
        }

        var ownedItem = cart.Items.FirstOrDefault(cartItem =>
            ownedPurchases.Any(purchase => purchase.Reference == cartItem.Reference));
        if (ownedItem is not null)
        {
            throw new DomainException($"You already own {ownedItem.Name}.");
        }

        var purchases = new List<Purchase>();
        foreach (var item in cart.Items)
        {
            var soldProduct = products.Single(product => product.Reference == item.Reference);
            var sellerWallet = sellerWallets[soldProduct.OwnerId];

            buyerWallet.PayFor(item.Reference, item.Name, item.Price, soldProduct.OwnerId);
            sellerWallet.ReceivePayment(item.Reference, item.Name, item.Price);
            purchases.Add(new Purchase(cart.OwnerId, item.Reference, item.Price));
        }

        cart.Clear();
        return purchases;
    }
}
