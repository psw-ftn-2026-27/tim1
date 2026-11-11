using Shared.Domain.Exceptions;
using Social.Application.Blogs;
using Social.Application.Images;
using Social.Domain.Blogs;
using Social.Domain.Images;

namespace Social.Application.BlogAuthoring;

public sealed class BlogAuthoringService
{
    private readonly IBlogRepository _blogRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly IImageStorage _imageStorage;
    private readonly IUnitOfWork _unitOfWork;

    public BlogAuthoringService(
        IBlogRepository blogRepository,
        IImageRepository imageRepository,
        IImageProcessor imageProcessor,
        IImageStorage imageStorage,
        IUnitOfWork unitOfWork)
    {
        _blogRepository = blogRepository;
        _imageRepository = imageRepository;
        _imageProcessor = imageProcessor;
        _imageStorage = imageStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(Guid authorId, CreateBlogDto dto)
    {
        var imageIds = dto.Images ?? [];
        var blog = new Blog(authorId, dto.Title, dto.Description, imageIds);
        var images = await _imageRepository.GetByIdsAsync(imageIds);
        foreach (var imageId in imageIds)
        {
            FindImage(images, imageId).AttachToBlog(authorId);
        }

        _blogRepository.Add(blog);
        await _unitOfWork.SaveChangesAsync();

        return blog.Id;
    }

    // The record is saved before the file is written, so a failure can leave a record
    // without a file, which the cleanup below removes later, but never a file without a
    // record (P19). An expired image loses its file before its record, for the same reason (P20).
    public async Task<Guid> UploadImageAsync(Guid userId, Stream content)
    {
        var processed = _imageProcessor.Process(content);
        var image = new Image(userId, processed.Width, processed.Height, processed.Content.Length);
        var expiredImages = (await _imageRepository.GetUnattachedAsync())
            .Where(unattached => unattached.IsExpired(DateTime.UtcNow))
            .ToList();

        foreach (var expired in expiredImages)
        {
            _imageStorage.Delete(expired.StoredFileName);
            _imageRepository.Remove(expired);
        }
        _imageRepository.Add(image);
        await _unitOfWork.SaveChangesAsync();

        await _imageStorage.SaveAsync(image.StoredFileName, processed.Content);

        return image.Id;
    }

    public async Task PublishAsync(Guid blogId, Guid authorId)
    {
        var blog = await GetOwnedBlogAsync(blogId, authorId);

        blog.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Blog> GetOwnedBlogAsync(Guid blogId, Guid authorId)
    {
        var blog = await _blogRepository.GetByIdAsync(blogId);
        if (blog is null || blog.AuthorId != authorId)
        {
            throw new NotFoundException($"Blog {blogId} does not exist.");
        }
        return blog;
    }

    private static Image FindImage(List<Image> images, Guid imageId)
    {
        var image = images.FirstOrDefault(image => image.Id == imageId);
        if (image is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }
        return image;
    }
}
