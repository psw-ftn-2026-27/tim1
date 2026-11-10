using Microsoft.EntityFrameworkCore;
using Social.Application.Images;
using Social.Domain.Blogs;

namespace Social.Infrastructure.Persistence.Images;

internal sealed class ImageReadRepository : IImageReadRepository
{
    private readonly SocialDbContext _dbContext;

    public ImageReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ImageFileDto?> GetOfAuthorsBlogAsync(Guid blogId, Guid imageId, Guid authorId)
    {
        return _dbContext.Images
            .Where(image => image.Id == imageId)
            .Where(image => _dbContext.Blogs.Any(blog =>
                blog.Id == blogId && blog.AuthorId == authorId && blog.Images.Contains(image.Id)))
            .Select(image => new ImageFileDto(image.StoredFileName))
            .FirstOrDefaultAsync();
    }

    public Task<ImageFileDto?> GetOfPublishedBlogAsync(Guid blogId, Guid imageId)
    {
        return _dbContext.Images
            .Where(image => image.Id == imageId)
            .Where(image => _dbContext.Blogs.Any(blog =>
                blog.Id == blogId && blog.Status == BlogStatus.Published && blog.Images.Contains(image.Id)))
            .Select(image => new ImageFileDto(image.StoredFileName))
            .FirstOrDefaultAsync();
    }
}
