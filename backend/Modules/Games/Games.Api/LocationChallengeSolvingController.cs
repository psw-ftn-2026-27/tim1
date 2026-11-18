using Games.Application.LocationChallengeSolving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "explorer")]
public sealed class LocationChallengeSolvingController : ControllerBase
{
    private readonly LocationChallengeSolvingService _solvingService;
    private readonly LocationChallengeSolvingQueries _solvingQueries;

    public LocationChallengeSolvingController(
        LocationChallengeSolvingService solvingService,
        LocationChallengeSolvingQueries solvingQueries)
    {
        _solvingService = solvingService;
        _solvingQueries = solvingQueries;
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<NearbyLocationChallengesDto>> GetNearby()
    {
        return await _solvingQueries.GetNearbyAsync(User.GetUserId());
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> Activate(Guid id)
    {
        await _solvingService.ActivateAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("{id:guid}/attempt")]
    public async Task<ActionResult<LocationChallengeAttemptDto>> GetAttempt(Guid id)
    {
        return await _solvingQueries.GetAttemptAsync(id, User.GetUserId());
    }

    // Returns the image file itself (image/webp), not a JSON DTO.
    [HttpGet("{id:guid}/attempt/images/{imageId:guid}")]
    public async Task<ActionResult> GetAttemptImage(Guid id, Guid imageId)
    {
        var image = await _solvingQueries.GetAttemptImageAsync(id, imageId, User.GetUserId());
        return File(image.Content, image.ContentType);
    }
}
