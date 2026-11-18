using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;
using Social.Application.ClubInviting;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize(Roles = "explorer")]
public sealed class ClubInvitingController : ControllerBase
{
    private readonly ClubInvitingService _invitingService;
    private readonly ClubInvitingQueries _invitingQueries;

    public ClubInvitingController(
        ClubInvitingService invitingService,
        ClubInvitingQueries invitingQueries)
    {
        _invitingService = invitingService;
        _invitingQueries = invitingQueries;
    }

    [HttpGet("invitations/search")]
    public async Task<ActionResult<PageResult<UserToInviteDto>>> SearchUsersToInvite([FromQuery] string text = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _invitingQueries.SearchUsersToInviteAsync(text, page, pageSize);
    }

    [HttpGet("invitations/mine")]
    public async Task<ActionResult<List<MyInvitationDto>>> GetMyInvitations()
    {
        return await _invitingQueries.GetMyInvitationsAsync(User.GetUserId());
    }

    [HttpGet("{id:guid}/invitations")]
    public async Task<ActionResult<List<SentInvitationDto>>> GetSentInvitations(Guid id)
    {
        return await _invitingQueries.GetSentInvitationsAsync(id, User.GetUserId());
    }

    [HttpPost("{id:guid}/invitations/{userId:guid}")]
    public async Task<ActionResult> InviteMember(Guid id, Guid userId)
    {
        await _invitingService.InviteMemberAsync(id, User.GetUserId(), userId);
        return NoContent();
    }

    [HttpPut("{id:guid}/invitations/mine/accept")]
    public async Task<ActionResult> AcceptInvitation(Guid id)
    {
        await _invitingService.AcceptInvitationAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPut("{id:guid}/invitations/mine/decline")]
    public async Task<ActionResult> DeclineInvitation(Guid id)
    {
        await _invitingService.DeclineInvitationAsync(id, User.GetUserId());
        return NoContent();
    }
}
