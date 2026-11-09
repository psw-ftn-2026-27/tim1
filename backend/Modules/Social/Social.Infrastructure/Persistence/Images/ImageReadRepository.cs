using Microsoft.EntityFrameworkCore;
using Social.Application.Images;
using Social.Domain.Blogs;
using Social.Domain.Images;

namespace Social.Infrastructure.Persistence.Images;

internal sealed class ImageReadRepository : IImageReadRepository
{
    private readonly SocialDbContext _dbContext;

    public ImageReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ImageFileDto?> GetOwnUnattachedAsync(Guid imageId, Guid userId)
    {
        return _dbContext.Images
            .Where(image => image.Id == imageId && image.UploaderId == userId && image.Status == ImageStatus.Unattached)
            .Select(image => new ImageFileDto(image.StoredFileName))
            .FirstOrDefaultAsync();
    }

    public Task<ImageFileDto?> GetOfAuthorsBlogAsync(Guid blogId, Guid imageId, Guid authorId)
    {
        var imageText = imageId.ToString();
        return _dbContext.Images
            .Where(image => image.Id == imageId)
            .Where(image => _dbContext.Blogs.Any(blog =>
                blog.Id == blogId && blog.AuthorId == authorId && blog.Images.Contains(imageText)))
            .Select(image => new ImageFileDto(image.StoredFileName))
            .FirstOrDefaultAsync();
    }

    public Task<ImageFileDto?> GetOfPublishedBlogAsync(Guid blogId, Guid imageId)
    {
        var imageText = imageId.ToString();
        return _dbContext.Images
            .Where(image => image.Id == imageId)
            .Where(image => _dbContext.Blogs.Any(blog =>
                blog.Id == blogId && blog.Status == BlogStatus.Published && blog.Images.Contains(imageText)))
            .Select(image => new ImageFileDto(image.StoredFileName))
            .FirstOrDefaultAsync();
    }
}
