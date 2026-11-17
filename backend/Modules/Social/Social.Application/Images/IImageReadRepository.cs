namespace Social.Application.Images;

public interface IImageReadRepository
{
    Task<ImageFileDto?> GetOfAuthorsBlogAsync(Guid blogId, Guid imageId, Guid authorId);
    Task<ImageFileDto?> GetOfVisibleBlogAsync(Guid blogId, Guid imageId);
}
