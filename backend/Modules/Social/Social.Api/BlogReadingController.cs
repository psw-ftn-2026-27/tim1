using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;
using Social.Application.BlogReading;
using Social.Application.Blogs;

namespace Social.Api;

[ApiController]
[Route("api/social/blogs")]
[Authorize]
public sealed class BlogReadingController : ControllerBase
{
    private readonly BlogReadingQueries _readingQueries;

    public BlogReadingController(BlogReadingQueries readingQueries)
    {
        _readingQueries = readingQueries;
    }

    [HttpGet("published")]
    public async Task<ActionResult<PageResult<BlogDto>>> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _readingQueries.GetPublishedAsync(page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BlogDto>> GetById(Guid id)
    {
        return await _readingQueries.GetByIdAsync(id);
    }

    // Returns the image file itself (image/webp), not a JSON DTO.
    [HttpGet("{blogId:guid}/images/{imageId:guid}")]
    public async Task<ActionResult> GetPublishedBlogImage(Guid blogId, Guid imageId)
    {
        var image = await _readingQueries.GetPublishedBlogImageAsync(blogId, imageId);
        return File(image.Content, image.ContentType);
    }
}
