using Payment.Application.Coupons;
using Payment.Application.GiftCards;
using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Shared.Domain.Exceptions;

namespace Payment.Application.Checkout;

/// <summary>
/// Finds the coupon and the gift card the customer typed or picked, for the checkout and for
/// the price shown before it. Nothing given means nothing found; something given that does
/// not exist is an error.
/// </summary>
public sealed class CheckoutDiscounts
{
    private readonly ICouponRepository _couponRepository;
    private readonly IGiftCardRepository _giftCardRepository;

    public CheckoutDiscounts(ICouponRepository couponRepository, IGiftCardRepository giftCardRepository)
    {
        _couponRepository = couponRepository;
        _giftCardRepository = giftCardRepository;
    }

    public async Task<Coupon?> FindCouponAsync(string? couponCode)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
        {
            return null;
        }

        return await _couponRepository.GetByCodeAsync(couponCode)
            ?? throw new NotFoundException("The coupon does not exist.");
    }

    public async Task<GiftCard?> FindGiftCardAsync(Guid? giftCardId)
    {
        if (giftCardId is null)
        {
            return null;
        }

        return await _giftCardRepository.GetByIdAsync(giftCardId.Value)
            ?? throw new NotFoundException("The gift card does not exist.");
    }
}
