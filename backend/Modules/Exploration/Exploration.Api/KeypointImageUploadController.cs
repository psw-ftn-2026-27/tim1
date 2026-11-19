using Exploration.Application.KeypointImageUpload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours/keypoint-images")]
[Authorize(Roles = "explorer")]
public sealed class KeypointImageUploadController : ControllerBase
{
    private readonly KeypointImageUploadService _imageUploadService;

    public KeypointImageUploadController(KeypointImageUploadService imageUploadService)
    {
        _imageUploadService = imageUploadService;
    }

    // The request body limit is global (Host.Api), so this endpoint needs no size attribute;
    // the shared image processor checks the file size again while reading it.
    [HttpPost]
    public async Task<ActionResult<Guid>> UploadKeypointImage(IFormFile file)
    {
        await using var content = file.OpenReadStream();
        return await _imageUploadService.UploadKeypointImageAsync(User.GetUserId(), content);
    }
}
