using AutoMapper;
using Shared.Domain.Exceptions;
using Social.Application.Blogs;
using Social.Application.Images;
using Social.Domain.Blogs;
using Social.Domain.Images;

namespace Social.Application.BlogAuthoring;

public sealed class BlogAuthoringService
{
    private readonly IBlogRepository _blogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IImageRepository _imageRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly IImageStorage _imageStorage;

    public BlogAuthoringService(
        IBlogRepository blogRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IImageRepository imageRepository,
        IImageProcessor imageProcessor,
        IImageStorage imageStorage)
    {
        _blogRepository = blogRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _imageRepository = imageRepository;
        _imageProcessor = imageProcessor;
        _imageStorage = imageStorage;
    }

    public async Task<BlogDto> CreateAsync(Guid authorId, CreateBlogDto dto)
    {
        var imageIds = new List<Guid>();
        foreach (var text in dto.Images ?? [])
        {
            if (!Guid.TryParse(text, out var imageId))
            {
                throw new NotFoundException($"Image {text} does not exist.");
            }
            imageIds.Add(imageId);
        }
        imageIds = imageIds.Distinct().ToList();

        var images = await _imageRepository.GetByIdsAsync(imageIds);
        if (images.Count != imageIds.Count)
        {
            throw new NotFoundException("One of the images does not exist.");
        }

        foreach (var image in images)
        {
            image.AttachTo(authorId);
        }
        var blog = new Blog(authorId, dto.Title, dto.Description, imageIds.Select(id => id.ToString()).ToList());

        _blogRepository.Add(blog);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BlogDto>(blog);
    }

    public async Task<Guid> UploadImageAsync(Guid userId, Stream content)
    {
        var processed = _imageProcessor.Process(content);
        var image = new Image(userId, processed.OriginalDimensions, processed.Content.Length);
        var expiredImages = (await _imageRepository.GetUnattachedAsync())
            .Where(unattached => unattached.IsExpired(DateTime.UtcNow))
            .ToList();

        _imageRepository.Add(image);
        foreach (var expired in expiredImages)
        {
            _imageRepository.Remove(expired);
        }
        await _unitOfWork.SaveChangesAsync();

        await _imageStorage.SaveAsync(image.StoredFileName, processed.Content);
        foreach (var expired in expiredImages)
        {
            _imageStorage.Delete(expired.StoredFileName);
        }

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
}
