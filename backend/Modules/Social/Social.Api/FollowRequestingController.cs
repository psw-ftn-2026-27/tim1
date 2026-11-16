using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;
using Social.Application.FollowRequesting;

namespace Social.Api;

[ApiController]
[Route("api/social/follow-requests")]
[Authorize(Roles = "explorer")]
public sealed class FollowRequestingController : ControllerBase
{
    private readonly FollowRequestingService _requestingService;
    private readonly FollowRequestingQueries _requestingQueries;

    public FollowRequestingController(
        FollowRequestingService requestingService,
        FollowRequestingQueries requestingQueries)
    {
        _requestingService = requestingService;
        _requestingQueries = requestingQueries;
    }

    [HttpPost("{recipientId:guid}")]
    public async Task<ActionResult> Send(Guid recipientId)
    {
        await _requestingService.SendFollowRequestAsync(User.GetUserId(), recipientId);
        return NoContent();
    }

    [HttpDelete("{requestId:guid}")]
    public async Task<ActionResult> Delete(Guid requestId)
    {
        await _requestingService.DeleteFollowRequestAsync(User.GetUserId(), requestId);
        return NoContent();
    }

    [HttpPut("{requestId:guid}/accept")]
    public async Task<ActionResult> Accept(Guid requestId)
    {
        await _requestingService.AcceptFollowRequestAsync(User.GetUserId(), requestId);
        return NoContent();
    }

    [HttpPut("{requestId:guid}/reject")]
    public async Task<ActionResult> Reject(Guid requestId)
    {
        await _requestingService.RejectFollowRequestAsync(User.GetUserId(), requestId);
        return NoContent();
    }

    [HttpGet("received")]
    public async Task<ActionResult<PageResult<ReceivedFollowRequestDto>>> GetReceived([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _requestingQueries.GetReceivedFollowRequestsAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("received/pending-count")]
    public async Task<ActionResult<int>> GetPendingReceivedCount()
    {
        return await _requestingQueries.GetPendingReceivedCountAsync(User.GetUserId());
    }

    [HttpGet("sent")]
    public async Task<ActionResult<List<FollowRequestDto>>> GetSent()
    {
        return await _requestingQueries.GetSentFollowRequestsAsync(User.GetUserId());
    }

    [HttpGet("authors")]
    public async Task<ActionResult<PageResult<AuthorToFollowDto>>> GetAuthors([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _requestingQueries.GetAuthorsToFollowAsync(User.GetUserId(), page, pageSize);
    }
}
