using Exploration.Application.KeypointAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize(Roles = "explorer")]
public sealed class KeypointAuthoringController : ControllerBase
{
    private readonly KeypointAuthoringService _keypointService;
    private readonly KeypointAuthoringQueries _keypointQueries;

    public KeypointAuthoringController(
        KeypointAuthoringService keypointService,
        KeypointAuthoringQueries keypointQueries)
    {
        _keypointService = keypointService;
        _keypointQueries = keypointQueries;
    }

    // The request body limit is global (Host.Api), so this endpoint needs no size attribute;
    // the shared image processor checks the file size again while reading it.
    [HttpPost("keypoint-images")]
    public async Task<ActionResult<Guid>> UploadKeypointImage(IFormFile file)
    {
        await using var content = file.OpenReadStream();
        return await _keypointService.UploadKeypointImageAsync(User.GetUserId(), content);
    }

    [HttpPost("{id:guid}/keypoints")]
    public async Task<ActionResult> AddKeypoint(Guid id, AddKeypointDto dto)
    {
        await _keypointService.AddKeypointAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/keypoints/{keypointId:guid}")]
    public async Task<ActionResult> UpdateKeypoint(Guid id, Guid keypointId, UpdateKeypointDto dto)
    {
        await _keypointService.UpdateKeypointAsync(id, User.GetUserId(), keypointId, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}/keypoints/{keypointId:guid}")]
    public async Task<ActionResult> RemoveKeypoint(Guid id, Guid keypointId)
    {
        await _keypointService.RemoveKeypointAsync(id, User.GetUserId(), keypointId);
        return NoContent();
    }

    [HttpPut("{id:guid}/keypoints/{keypointId:guid}/visibility")]
    public async Task<ActionResult> SetKeypointVisibility(Guid id, Guid keypointId, SetKeypointVisibilityDto dto)
    {
        await _keypointService.SetKeypointVisibilityAsync(id, User.GetUserId(), keypointId, dto);
        return NoContent();
    }

    [HttpGet("{id:guid}/keypoints")]
    public async Task<ActionResult<List<KeypointDto>>> GetKeypoints(Guid id)
    {
        return await _keypointQueries.GetKeypointsAsync(id, User.GetUserId());
    }

    [HttpGet("{id:guid}/keypoints/{keypointId:guid}")]
    public async Task<ActionResult<KeypointDto>> GetKeypoint(Guid id, Guid keypointId)
    {
        return await _keypointQueries.GetKeypointAsync(id, User.GetUserId(), keypointId);
    }

    // Returns the image file itself (image/webp), not a JSON DTO.
    [HttpGet("{id:guid}/keypoints/{keypointId:guid}/image")]
    public async Task<ActionResult> GetKeypointImage(Guid id, Guid keypointId)
    {
        var image = await _keypointQueries.GetKeypointImageAsync(id, User.GetUserId(), keypointId);
        return File(image.Content, image.ContentType);
    }
}
