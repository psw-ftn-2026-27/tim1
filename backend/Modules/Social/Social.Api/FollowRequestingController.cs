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
