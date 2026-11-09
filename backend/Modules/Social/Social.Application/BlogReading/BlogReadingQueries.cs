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
        if (blog is null || blog.Status != BlogStatus.Published)
        {
            throw new NotFoundException($"Blog {id} does not exist.");
        }

        return blog;
    }

    public Task<PageResult<BlogDto>> GetPublishedAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _blogReadRepository.GetPublishedAsync(page, pageSize);
    }

    public async Task<ImageContentDto> GetPublishedImageAsync(Guid blogId, Guid imageId)
    {
        var file = await _imageReadRepository.GetOfPublishedBlogAsync(blogId, imageId);
        if (file is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }

        return await _imageStorage.ReadAsync(file.StoredFileName);
    }
}
