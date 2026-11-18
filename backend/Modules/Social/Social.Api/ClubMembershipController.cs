using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubMembership;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize]
public sealed class ClubMembershipController : ControllerBase
{
    private readonly ClubMembershipService _membershipService;
    private readonly ClubMembershipQueries _membershipQueries;

    public ClubMembershipController(
        ClubMembershipService membershipService,
        ClubMembershipQueries membershipQueries)
    {
        _membershipService = membershipService;
        _membershipQueries = membershipQueries;
    }

    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<List<ClubMemberDto>>> GetMembers(Guid id)
    {
        return await _membershipQueries.GetMembersAsync(id);
    }

    [HttpGet("{id:guid}/membership")]
    public async Task<ActionResult<MyMembershipDto>> GetMyMembership(Guid id)
    {
        return await _membershipQueries.GetMyMembershipAsync(id, User.GetUserId());
    }

    [HttpGet("{id:guid}/membership-requests")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult<List<MembershipRequestDto>>> GetPendingRequests(Guid id)
    {
        return await _membershipQueries.GetPendingRequestsAsync(id, User.GetUserId());
    }

    [HttpPost("{id:guid}/membership-requests")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> RequestMembership(Guid id)
    {
        await _membershipService.RequestMembershipAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpDelete("{id:guid}/membership-requests/mine")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> WithdrawMembershipRequest(Guid id)
    {
        await _membershipService.WithdrawMembershipRequestAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPut("{id:guid}/membership-requests/{requesterId:guid}/accept")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> AcceptMembershipRequest(Guid id, Guid requesterId)
    {
        await _membershipService.AcceptMembershipRequestAsync(id, User.GetUserId(), requesterId);
        return NoContent();
    }

    [HttpPut("{id:guid}/membership-requests/{requesterId:guid}/reject")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> RejectMembershipRequest(Guid id, Guid requesterId)
    {
        await _membershipService.RejectMembershipRequestAsync(id, User.GetUserId(), requesterId);
        return NoContent();
    }
}
