using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Blogs;
using Social.Application.Images;
using Social.Domain.Blogs;

namespace Social.Application.BlogReading;

public sealed class BlogReadingQueries
{
    private readonly IBlogReadRepository _blogReadRepository;
    private readonly IImageReadRepository _imageReadRepository;
    private readonly IImageStorage _imageStorage;

    public BlogReadingQueries(
        IBlogReadRepository blogReadRepository,
        IImageReadRepository imageReadRepository,
        IImageStorage imageStorage)
    {
        _blogReadRepository = blogReadRepository;
        _imageReadRepository = imageReadRepository;
        _imageStorage = imageStorage;
    }

    public async Task<BlogDto> GetByIdAsync(Guid id)
    {
        var blog = await _blogReadRepository.GetByIdAsync(id);
        if (blog is null || (blog.Status != BlogStatus.Published && blog.Status != BlogStatus.Archived))
        {
            throw new NotFoundException($"Blog {id} does not exist.");
        }

        return blog;
    }

    public Task<PageResult<BlogDto>> GetVisibleAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _blogReadRepository.GetVisibleAsync(page, pageSize);
    }

    // The image must belong to this blog; otherwise a visible blog's ID would open the
    // image of any draft (P21).
    public async Task<ImageContentDto> GetVisibleBlogImageAsync(Guid blogId, Guid imageId)
    {
        var file = await _imageReadRepository.GetOfVisibleBlogAsync(blogId, imageId);
        if (file is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }

        var content = await _imageStorage.ReadAsync(file.StoredFileName);
        if (content is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }
        return content;
    }
}
