using Games.Application.ChallengeImages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/images")]
[Authorize(Roles = "administrator")]
public sealed class ChallengeImagesController : ControllerBase
{
    private readonly ChallengeImagesService _imagesService;
    private readonly ChallengeImagesQueries _imagesQueries;

    public ChallengeImagesController(ChallengeImagesService imagesService, ChallengeImagesQueries imagesQueries)
    {
        _imagesService = imagesService;
        _imagesQueries = imagesQueries;
    }

    // The request body limit is global (Host.Api), so this endpoint needs no size attribute;
    // the shared image processor checks the file size again while reading it.
    [HttpPost]
    public async Task<ActionResult<Guid>> UploadImage(IFormFile file)
    {
        await using var content = file.OpenReadStream();
        return await _imagesService.UploadImageAsync(content);
    }

    // Returns the image file itself (image/webp), not a JSON DTO.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetImage(Guid id)
    {
        var image = await _imagesQueries.GetImageAsync(id);
        return File(image.Content, image.ContentType);
    }
}
