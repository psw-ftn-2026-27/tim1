using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubManagement;
using Social.Application.Clubs;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize(Roles = "explorer")]
public sealed class ClubManagementController : ControllerBase
{
    private readonly ClubManagementService _managementService;
    private readonly ClubManagementQueries _managementQueries;

    public ClubManagementController(
        ClubManagementService managementService,
        ClubManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<ClubDto>>> GetMine()
    {
        return await _managementQueries.GetByOwnerAsync(User.GetUserId());
    }

    [HttpPost]
    public async Task<ActionResult<ClubDto>> Create(CreateClubDto dto)
    {
        return await _managementService.CreateAsync(User.GetUserId(), dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateClubDto dto)
    {
        await _managementService.UpdateAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _managementService.DeleteAsync(id, User.GetUserId());
        return NoContent();
    }
}
