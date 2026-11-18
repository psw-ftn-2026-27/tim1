using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.GiftCards;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/gift-cards")]
[Authorize(Roles = "explorer")]
public sealed class GiftCardsController : ControllerBase
{
    private readonly GiftCardsService _giftCardsService;
    private readonly GiftCardsQueries _giftCardsQueries;

    public GiftCardsController(GiftCardsService giftCardsService, GiftCardsQueries giftCardsQueries)
    {
        _giftCardsService = giftCardsService;
        _giftCardsQueries = giftCardsQueries;
    }

    [HttpGet]
    public async Task<ActionResult<GiftCardsDto>> GetGiftCards()
    {
        return await _giftCardsQueries.GetGiftCardsAsync(User.GetUserId());
    }

    [HttpGet("recipients")]
    public async Task<ActionResult<PageResult<RecipientDto>>> SearchRecipients([FromQuery] string text = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _giftCardsQueries.SearchRecipientsAsync(User.GetUserId(), text, page, pageSize);
    }

    [HttpGet("recipients/{recipientId:guid}")]
    public async Task<ActionResult<RecipientDto>> GetRecipient(Guid recipientId)
    {
        return await _giftCardsQueries.GetRecipientAsync(recipientId);
    }

    [HttpGet("received/valid-count")]
    public async Task<ActionResult<int>> GetValidReceivedCount()
    {
        return await _giftCardsQueries.GetValidReceivedCountAsync(User.GetUserId());
    }

    [HttpPost]
    public async Task<ActionResult> PurchaseGiftCard(PurchaseGiftCardDto dto)
    {
        await _giftCardsService.PurchaseGiftCardAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
