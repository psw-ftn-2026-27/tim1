using Exploration.Application.UserPositioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/user-positions")]
[Authorize(Roles = "explorer")]
public sealed class UserPositioningController : ControllerBase
{
    private readonly UserPositioningService _positioningService;
    private readonly UserPositioningQueries _positioningQueries;

    public UserPositioningController(UserPositioningService positioningService, UserPositioningQueries positioningQueries)
    {
        _positioningService = positioningService;
        _positioningQueries = positioningQueries;
    }

    [HttpPut("mine")]
    public async Task<ActionResult> SaveMine(SaveCurrentPositionDto dto)
    {
        await _positioningService.SaveCurrentPositionAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<ActionResult<CurrentPositionDto?>> GetMine()
    {
        return await _positioningQueries.GetMyCurrentPositionAsync(User.GetUserId());
    }
}
