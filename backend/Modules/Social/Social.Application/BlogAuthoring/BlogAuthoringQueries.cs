using Shared.Domain.Exceptions;
using Social.Application.Blogs;
using Social.Application.Images;

namespace Social.Application.BlogAuthoring;

public sealed class BlogAuthoringQueries
{
    private readonly IBlogReadRepository _blogReadRepository;
    private readonly IImageReadRepository _imageReadRepository;
    private readonly IImageStorage _imageStorage;

    public BlogAuthoringQueries(
        IBlogReadRepository blogReadRepository,
        IImageReadRepository imageReadRepository,
        IImageStorage imageStorage)
    {
        _blogReadRepository = blogReadRepository;
        _imageReadRepository = imageReadRepository;
        _imageStorage = imageStorage;
    }

    public Task<List<BlogDto>> GetByAuthorAsync(Guid authorId)
    {
        return _blogReadRepository.GetByAuthorAsync(authorId);
    }

    public async Task<ImageContentDto> GetOwnBlogImageAsync(Guid blogId, Guid imageId, Guid userId)
    {
        var file = await _imageReadRepository.GetOfAuthorsBlogAsync(blogId, imageId, userId);
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
