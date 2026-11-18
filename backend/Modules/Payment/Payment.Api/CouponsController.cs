using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Coupons;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/coupons")]
[Authorize(Roles = "explorer")]
public sealed class CouponsController : ControllerBase
{
    private readonly CouponsService _couponsService;
    private readonly CouponsQueries _couponsQueries;

    public CouponsController(CouponsService couponsService, CouponsQueries couponsQueries)
    {
        _couponsService = couponsService;
        _couponsQueries = couponsQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<CouponDto>>> GetCoupons()
    {
        return await _couponsQueries.GetCouponsAsync(User.GetUserId());
    }

    [HttpGet("owned-products")]
    public async Task<ActionResult<List<OwnedProductDto>>> GetOwnedProducts()
    {
        return await _couponsQueries.GetOwnedProductsAsync(User.GetUserId());
    }

    [HttpPost]
    public async Task<ActionResult> CreateCoupon(CreateCouponDto dto)
    {
        await _couponsService.CreateCouponAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
