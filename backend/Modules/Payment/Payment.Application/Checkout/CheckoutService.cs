using Payment.Application.Products;
using Payment.Application.Purchases;
using Payment.Application.ShoppingCarts;
using Payment.Application.Wallets;
using Payment.Domain.Products;
using Payment.Domain.Purchases;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.Checkout;

public sealed class CheckoutService
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly CheckoutDiscounts _discounts;
    private readonly CartCheckout _cartCheckout;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutService(
        IShoppingCartRepository cartRepository,
        IProductRepository productRepository,
        IWalletRepository walletRepository,
        IPurchaseRepository purchaseRepository,
        CheckoutDiscounts discounts,
        CartCheckout cartCheckout,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _walletRepository = walletRepository;
        _purchaseRepository = purchaseRepository;
        _discounts = discounts;
        _cartCheckout = cartCheckout;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Pays for everything the customer has in their cart. The wallets, the purchases and the
    /// emptied cart are written by the single save at the end, so a checkout the domain refuses
    /// leaves the database untouched. The coupon and the gift card, when given, are looked up
    /// here and spent by the domain, which prices the cart itself instead of trusting a price
    /// from the client.
    /// </summary>
    public async Task CheckoutAsync(Guid customerId, string? couponCode, Guid? giftCardId)
    {
        var cart = await _cartRepository.GetByOwnerAsync(customerId);
        if (cart is null)
        {
            throw new NotFoundException($"Cart of owner {customerId} does not exist.");
        }

        var references = cart.Items.Select(item => item.Reference).ToList();
        var products = await ProductsOfAsync(references);
        var buyerWallet = await WalletOfAsync(customerId);
        var sellerWallets = await SellerWalletsOfAsync(products);
        var ownedPurchases = await _purchaseRepository.GetOfBuyerAsync(customerId, references);

        var coupon = await _discounts.FindCouponAsync(couponCode);
        var giftCard = await _discounts.FindGiftCardAsync(giftCardId);

        var purchases = _cartCheckout.Checkout(
            cart, products, buyerWallet, sellerWallets, ownedPurchases, coupon, giftCard);
        foreach (var purchase in purchases)
        {
            _purchaseRepository.Add(purchase);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>The products the cart items stand for; an item the offer no longer holds is an error.</summary>
    private async Task<List<Product>> ProductsOfAsync(IReadOnlyList<ProductReference> references)
    {
        var products = await _productRepository.GetByReferencesAsync(references);

        var missing = references.FirstOrDefault(reference =>
            !products.Any(product => product.Reference == reference));
        if (missing is not null)
        {
            throw new NotFoundException($"Product {missing.EntityType} {missing.EntityId} is not registered.");
        }

        return products;
    }

    /// <summary>The wallet of every owner who earns from the cart, one entry per owner.</summary>
    private async Task<IReadOnlyDictionary<Guid, Wallet>> SellerWalletsOfAsync(IReadOnlyList<Product> products)
    {
        var sellerWallets = new Dictionary<Guid, Wallet>();
        foreach (var ownerId in products.Select(product => product.OwnerId).Distinct())
        {
            sellerWallets.Add(ownerId, await WalletOfAsync(ownerId));
        }

        return sellerWallets;
    }

    /// <summary>
    /// The wallet of an owner, opened empty when they have none, the way a top-up request opens
    /// one. An owner who sells first gets their wallet here; a customer who buys first gets an
    /// empty one, so the domain refuses the payment for want of funds.
    /// </summary>
    private async Task<Wallet> WalletOfAsync(Guid ownerId)
    {
        var wallet = await _walletRepository.GetByOwnerAsync(ownerId);
        if (wallet is null)
        {
            wallet = new Wallet(ownerId);
            _walletRepository.Add(wallet);
        }

        return wallet;
    }
}
