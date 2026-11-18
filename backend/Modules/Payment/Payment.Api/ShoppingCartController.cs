using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Products;
using Payment.Application.ShoppingCarts;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/cart")]
[Authorize(Roles = "explorer")]
public sealed class ShoppingCartController : ControllerBase
{
    private readonly ShoppingCartService _cartService;
    private readonly ShoppingCartQueries _cartQueries;

    public ShoppingCartController(ShoppingCartService cartService, ShoppingCartQueries cartQueries)
    {
        _cartService = cartService;
        _cartQueries = cartQueries;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        return await _cartQueries.GetCartAsync(User.GetUserId());
    }

    [HttpGet("item-count")]
    public async Task<ActionResult<int>> GetItemCount()
    {
        return await _cartQueries.GetCartItemCountAsync(User.GetUserId());
    }

    [HttpPost("items")]
    public async Task<ActionResult> AddItem(ProductReferenceDto dto)
    {
        await _cartService.AddItemAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("items/{type}/{entityId:guid}")]
    public async Task<ActionResult> RemoveItem([FromRoute] ProductReferenceDto dto)
    {
        await _cartService.RemoveItemAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
