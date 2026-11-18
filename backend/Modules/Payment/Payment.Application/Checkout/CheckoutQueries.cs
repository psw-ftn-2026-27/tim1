using Payment.Application.Coupons;
using Payment.Application.GiftCards;
using Payment.Application.Products;
using Payment.Application.ShoppingCarts;
using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;

namespace Payment.Application.Checkout;

public sealed class CheckoutQueries
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IGiftCardReadRepository _giftCardReadRepository;
    private readonly CartPricing _cartPricing;

    public CheckoutQueries(
        IShoppingCartRepository cartRepository,
        IProductRepository productRepository,
        ICouponRepository couponRepository,
        IGiftCardRepository giftCardRepository,
        IGiftCardReadRepository giftCardReadRepository,
        CartPricing cartPricing)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _giftCardRepository = giftCardRepository;
        _giftCardReadRepository = giftCardReadRepository;
        _cartPricing = cartPricing;
    }

    /// <summary>
    /// What the customer's cart would cost with the coupon and the card, worked out by the same
    /// domain service the checkout uses. Nothing is spent or written; a coupon or card that
    /// cannot be used is an error. A customer without a cart sees an empty one.
    /// </summary>
    public async Task<PriceDto> GetPriceAsync(Guid customerId, string? couponCode, Guid? giftCardId)
    {
        var cart = await _cartRepository.GetByOwnerAsync(customerId) ?? new ShoppingCart(customerId);
        var references = cart.Items.Select(item => item.Reference).ToList();
        var products = await _productRepository.GetByReferencesAsync(references);

        Coupon? coupon = null;
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            coupon = await _couponRepository.GetByCodeAsync(couponCode)
                ?? throw new NotFoundException("The coupon does not exist.");
        }

        GiftCard? giftCard = null;
        if (giftCardId is not null)
        {
            giftCard = await _giftCardRepository.GetByIdAsync(giftCardId.Value)
                ?? throw new NotFoundException("The gift card does not exist.");
        }

        var price = _cartPricing.Calculate(cart, products, coupon, giftCard);

        var couponProductName = cart.Items.FirstOrDefault(item => item.Reference == price.CouponReference)?.Name;
        return new PriceDto(
            price.Total, price.CouponDiscount, couponProductName, price.CardDiscount, price.FinalPrice, price.CardLost);
    }

    public Task<List<UsableGiftCardDto>> GetUsableGiftCardsAsync(Guid customerId)
    {
        return _giftCardReadRepository.GetValidReceivedAsync(customerId);
    }
}
