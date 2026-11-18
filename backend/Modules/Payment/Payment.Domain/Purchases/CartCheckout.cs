using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
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
/// An optional coupon and gift card lower the price, as <see cref="CartPricing"/> works out, and
/// are spent by the checkout. The coupon lowers the price of its item, which the seller earns
/// and the purchase keeps. The card lowers only what the buyer pays, taken from the items in
/// order, because the platform already holds the money the giver paid for it.
/// </summary>
public sealed class CartCheckout
{
    private readonly CartPricing _cartPricing = new();

    public IReadOnlyList<Purchase> Checkout(
        ShoppingCart cart,
        IReadOnlyList<Product> products,
        Wallet buyerWallet,
        IReadOnlyDictionary<Guid, Wallet> sellerWallets,
        IReadOnlyList<Purchase> ownedPurchases,
        Coupon? coupon = null,
        GiftCard? giftCard = null)
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

        var price = _cartPricing.Calculate(cart, products, coupon, giftCard);
        coupon?.Use(DateTime.UtcNow);
        giftCard?.Use(cart.OwnerId, DateTime.UtcNow);

        var cardLeft = price.CardDiscount;
        var purchases = new List<Purchase>();
        foreach (var item in cart.Items)
        {
            var soldProduct = products.Single(product => product.Reference == item.Reference);
            var sellerWallet = sellerWallets[soldProduct.OwnerId];

            var hasCoupon = coupon is not null && item.Reference == price.CouponReference;
            var couponDiscount = hasCoupon ? price.CouponDiscount : 0m;
            var itemPrice = item.Price - couponDiscount;
            var cardShare = Math.Min(cardLeft, itemPrice);
            cardLeft -= cardShare;

            buyerWallet.PayFor(item.Reference, item.Name, itemPrice - cardShare, soldProduct.OwnerId);
            sellerWallet.ReceivePayment(item.Reference, item.Name, itemPrice);
            purchases.Add(new Purchase(cart.OwnerId, item.Reference, itemPrice, hasCoupon ? coupon!.Id : null, couponDiscount));
        }

        cart.Clear();
        return purchases;
    }
}
