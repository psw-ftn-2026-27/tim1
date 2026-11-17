using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.WalletRequestReview;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet/requests")]
[Authorize(Roles = "administrator")]
public sealed class WalletRequestReviewController : ControllerBase
{
    private readonly WalletRequestReviewService _reviewService;
    private readonly WalletRequestReviewQueries _reviewQueries;

    public WalletRequestReviewController(
        WalletRequestReviewService reviewService,
        WalletRequestReviewQueries reviewQueries)
    {
        _reviewService = reviewService;
        _reviewQueries = reviewQueries;
    }

    [HttpGet]
    public async Task<ActionResult<WalletRequestsDto>> GetRequests()
    {
        return await _reviewQueries.GetRequestsAsync();
    }

    [HttpPost("top-ups/{id:guid}/approve")]
    public async Task<ActionResult> ApproveTopUp(Guid id)
    {
        await _reviewService.ApproveTopUpAsync(id);
        return NoContent();
    }

    [HttpPost("top-ups/{id:guid}/reject")]
    public async Task<ActionResult> RejectTopUp(Guid id, RejectRequestDto dto)
    {
        await _reviewService.RejectTopUpAsync(id, dto);
        return NoContent();
    }

    [HttpPost("withdrawals/{id:guid}/approve")]
    public async Task<ActionResult> ApproveWithdrawal(Guid id)
    {
        await _reviewService.ApproveWithdrawalAsync(id);
        return NoContent();
    }

    [HttpPost("withdrawals/{id:guid}/reject")]
    public async Task<ActionResult> RejectWithdrawal(Guid id, RejectRequestDto dto)
    {
        await _reviewService.RejectWithdrawalAsync(id, dto);
        return NoContent();
    }
}
