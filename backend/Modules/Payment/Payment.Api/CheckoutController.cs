using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Checkout;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/cart")]
[Authorize(Roles = "explorer")]
public sealed class CheckoutController : ControllerBase
{
    private readonly CheckoutService _checkoutService;
    private readonly CheckoutQueries _checkoutQueries;

    public CheckoutController(CheckoutService checkoutService, CheckoutQueries checkoutQueries)
    {
        _checkoutService = checkoutService;
        _checkoutQueries = checkoutQueries;
    }

    [HttpGet("price")]
    public async Task<ActionResult<PriceDto>> GetPrice([FromQuery] string? couponCode, [FromQuery] Guid? giftCardId)
    {
        return await _checkoutQueries.GetPriceAsync(User.GetUserId(), couponCode, giftCardId);
    }

    [HttpGet("usable-gift-cards")]
    public async Task<ActionResult<List<UsableGiftCardDto>>> GetUsableGiftCards()
    {
        return await _checkoutQueries.GetUsableGiftCardsAsync(User.GetUserId());
    }

    // The body is optional, so a client that posts nothing still buys the cart without a discount.
    [HttpPost("checkout")]
    public async Task<ActionResult> Checkout([FromBody] CheckoutDto? dto = null)
    {
        await _checkoutService.CheckoutAsync(User.GetUserId(), dto?.CouponCode, dto?.GiftCardId);
        return NoContent();
    }
}
