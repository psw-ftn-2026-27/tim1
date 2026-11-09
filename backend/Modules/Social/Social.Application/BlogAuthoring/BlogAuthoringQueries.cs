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

    public async Task<ImageContentDto> GetOwnUnattachedImageAsync(Guid imageId, Guid userId)
    {
        var file = await _imageReadRepository.GetOwnUnattachedAsync(imageId, userId);
        return await ReadAsync(file, imageId);
    }

    public async Task<ImageContentDto> GetDraftImageAsync(Guid blogId, Guid imageId, Guid authorId)
    {
        var file = await _imageReadRepository.GetOfAuthorsBlogAsync(blogId, imageId, authorId);
        return await ReadAsync(file, imageId);
    }

    private async Task<ImageContentDto> ReadAsync(ImageFileDto? file, Guid imageId)
    {
        if (file is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }

        return await _imageStorage.ReadAsync(file.StoredFileName);
    }
}
