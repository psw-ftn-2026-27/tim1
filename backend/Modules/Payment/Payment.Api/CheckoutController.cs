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

    public CheckoutController(CheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult> Checkout()
    {
        await _checkoutService.CheckoutAsync(User.GetUserId());
        return NoContent();
    }
}
