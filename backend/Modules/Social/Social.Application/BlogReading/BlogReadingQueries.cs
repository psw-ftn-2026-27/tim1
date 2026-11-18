using Identity.Contracts;
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
    private readonly IIdentityApi _identityApi;

    public BlogReadingQueries(
        IBlogReadRepository blogReadRepository,
        IImageReadRepository imageReadRepository,
        IImageStorage imageStorage,
        IIdentityApi identityApi)
    {
        _blogReadRepository = blogReadRepository;
        _imageReadRepository = imageReadRepository;
        _imageStorage = imageStorage;
        _identityApi = identityApi;
    }

    public async Task<BlogDetailDto> GetByIdAsync(Guid id, Guid userId)
    {
        var blog = await _blogReadRepository.GetDetailByIdAsync(id, userId, DateTime.UtcNow);
        if (blog is null || (blog.Status != BlogStatus.Published && blog.Status != BlogStatus.Archived))
        {
            throw new NotFoundException($"Blog {id} does not exist.");
        }

        var authorIds = blog.Comments.Select(comment => comment.UserId).Distinct().ToList();
        var authors = await _identityApi.GetUsersAsync(authorIds);
        var displayNames = authors.ToDictionary(author => author.Id, author => author.DisplayName);

        var comments = blog.Comments
            .Select(comment => comment with { AuthorName = displayNames.GetValueOrDefault(comment.UserId) })
            .ToList();

        return blog with { Comments = comments };
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
