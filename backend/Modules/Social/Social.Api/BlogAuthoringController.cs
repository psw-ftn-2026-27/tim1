using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.BlogAuthoring;
using Social.Application.Blogs;
using Social.Application.Images;

namespace Social.Api;

[ApiController]
[Route("api/social/blogs")]
[Authorize(Roles = "explorer")]
public sealed class BlogAuthoringController : ControllerBase
{
    private readonly BlogAuthoringService _authoringService;
    private readonly BlogAuthoringQueries _authoringQueries;

    public BlogAuthoringController(
        BlogAuthoringService authoringService,
        BlogAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpPost]
    public async Task<ActionResult<BlogDto>> Create(CreateBlogDto dto)
    {
        return await _authoringService.CreateAsync(User.GetUserId(), dto);
    }

    // The limit is the 5 MB file plus headroom for the multipart envelope; the exact file
    // size is checked again by the image processor.
    [HttpPost("images")]
    [RequestSizeLimit(5 * 1024 * 1024 + 4096)]
    public async Task<ActionResult<Guid>> UploadImage(IFormFile file)
    {
        await using var content = file.OpenReadStream();
        return await _authoringService.UploadImageAsync(User.GetUserId(), content);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(Guid id)
    {
        await _authoringService.PublishAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<BlogDto>>> GetMine()
    {
        return await _authoringQueries.GetByAuthorAsync(User.GetUserId());
    }

    [HttpGet("images/{imageId:guid}")]
    public async Task<ActionResult<ImageContentDto>> GetOwnUnattachedImage(Guid imageId)
    {
        var image = await _authoringQueries.GetOwnUnattachedImageAsync(imageId, User.GetUserId());
        return File(image.Content, image.ContentType);
    }

    [HttpGet("mine/{blogId:guid}/images/{imageId:guid}")]
    public async Task<ActionResult<ImageContentDto>> GetDraftImage(Guid blogId, Guid imageId)
    {
        var image = await _authoringQueries.GetDraftImageAsync(blogId, imageId, User.GetUserId());
        return File(image.Content, image.ContentType);
    }
}
