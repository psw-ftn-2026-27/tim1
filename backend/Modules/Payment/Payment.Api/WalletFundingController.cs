using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.WalletFunding;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet")]
[Authorize(Roles = "explorer")]
public sealed class WalletFundingController : ControllerBase
{
    private readonly WalletFundingService _fundingService;
    private readonly WalletFundingQueries _fundingQueries;

    public WalletFundingController(
        WalletFundingService fundingService,
        WalletFundingQueries fundingQueries)
    {
        _fundingService = fundingService;
        _fundingQueries = fundingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<WalletOverviewDto>> GetOverview()
    {
        return await _fundingQueries.GetOverviewAsync(User.GetUserId());
    }

    [HttpPost("top-up-requests")]
    public async Task<ActionResult> RequestTopUp(RequestTopUpDto dto)
    {
        await _fundingService.RequestTopUpAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
