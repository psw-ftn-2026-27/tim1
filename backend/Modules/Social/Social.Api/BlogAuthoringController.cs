using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.BlogAuthoring;
using Social.Application.Blogs;

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
    public async Task<ActionResult<Guid>> Create(CreateBlogDto dto)
    {
        return await _authoringService.CreateAsync(User.GetUserId(), dto);
    }

    // The request body limit is global (Host.Api), so this endpoint needs no size attribute;
    // the shared image processor checks the file size again while reading it.
    [HttpPost("images")]
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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateBlogDto dto)
    {
        await _authoringService.UpdateAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<BlogDto>>> GetMine()
    {
        return await _authoringQueries.GetByAuthorAsync(User.GetUserId());
    }

    [HttpGet("mine/{id:guid}")]
    public async Task<ActionResult<BlogDto>> GetOwnBlogById(Guid id)
    {
        return await _authoringQueries.GetOwnBlogByIdAsync(id, User.GetUserId());
    }

    // Returns the image file itself (image/webp), not a JSON DTO.
    [HttpGet("mine/{blogId:guid}/images/{imageId:guid}")]
    public async Task<ActionResult> GetOwnBlogImage(Guid blogId, Guid imageId)
    {
        var image = await _authoringQueries.GetOwnBlogImageAsync(blogId, imageId, User.GetUserId());
        return File(image.Content, image.ContentType);
    }
}
