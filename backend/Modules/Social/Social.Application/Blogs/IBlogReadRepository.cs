using Shared.Domain;

namespace Social.Application.Blogs;

public interface IBlogReadRepository
{
    Task<BlogDto?> GetByIdAsync(Guid id);
    Task<List<BlogDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<BlogDto>> GetVisibleAsync(int page, int pageSize);
}
