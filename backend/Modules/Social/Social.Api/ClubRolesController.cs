using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubRoles;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize(Roles = "explorer")]
public sealed class ClubRolesController : ControllerBase
{
    private readonly ClubRolesService _rolesService;

    public ClubRolesController(ClubRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [HttpPut("{id:guid}/members/{memberId:guid}/promote")]
    public async Task<ActionResult> PromoteToModerator(Guid id, Guid memberId)
    {
        await _rolesService.PromoteToModeratorAsync(id, User.GetUserId(), memberId);
        return NoContent();
    }

    [HttpPut("{id:guid}/members/{memberId:guid}/transfer-ownership")]
    public async Task<ActionResult> TransferOwnership(Guid id, Guid memberId)
    {
        await _rolesService.TransferOwnershipAsync(id, User.GetUserId(), memberId);
        return NoContent();
    }
}
