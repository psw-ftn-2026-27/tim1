using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.BlogVoting;

namespace Social.Api;

[ApiController]
[Route("api/social/blogs")]
[Authorize]
public sealed class BlogVotingController : ControllerBase
{
    private readonly BlogVotingService _votingService;

    public BlogVotingController(BlogVotingService votingService)
    {
        _votingService = votingService;
    }

    [HttpPost("{id:guid}/votes")]
    public async Task<ActionResult> Vote(Guid id, VoteDto dto)
    {
        await _votingService.VoteAsync(id, User.GetUserId(), dto);
        return NoContent();
    }
}
