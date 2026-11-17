using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;
using Social.Application.FollowManagement;

namespace Social.Api;

[ApiController]
[Route("api/social")]
[Authorize(Roles = "explorer")]
public sealed class FollowManagementController : ControllerBase
{
    private readonly FollowManagementService _managementService;
    private readonly FollowManagementQueries _managementQueries;

    public FollowManagementController(
        FollowManagementService managementService,
        FollowManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpDelete("followers/{requestId:guid}")]
    public async Task<ActionResult> RemoveFollower(Guid requestId)
    {
        await _managementService.RemoveFollowerAsync(User.GetUserId(), requestId);
        return NoContent();
    }

    [HttpDelete("following/{requestId:guid}")]
    public async Task<ActionResult> StopFollowing(Guid requestId)
    {
        await _managementService.StopFollowingAsync(User.GetUserId(), requestId);
        return NoContent();
    }

    [HttpGet("followers")]
    public async Task<ActionResult<PageResult<FollowerDto>>> GetFollowers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _managementQueries.GetFollowersAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("following")]
    public async Task<ActionResult<PageResult<FollowingDto>>> GetFollowing([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _managementQueries.GetFollowingAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("follow-counts")]
    public async Task<ActionResult<FollowCountsDto>> GetFollowCounts()
    {
        return await _managementQueries.GetFollowCountsAsync(User.GetUserId());
    }
}
