using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.FollowRequesting;
using Social.Application.FollowRequests;
using Social.Domain.Blogs;
using Social.Domain.FollowRequests;

namespace Social.Infrastructure.Persistence.FollowRequests;

internal sealed class FollowRequestReadRepository : IFollowRequestReadRepository
{
    private readonly SocialDbContext _dbContext;

    public FollowRequestReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<FollowRequestDto>> GetSentAsync(Guid senderId)
    {
        return _dbContext.FollowRequests
            .Where(request => request.SenderId == senderId)
            .OrderByDescending(request => request.CreatedAt)
            .Select(request => new FollowRequestDto(request.Id, request.RecipientId, request.Status, request.CreatedAt))
            .ToListAsync();
    }

    public async Task<PageResult<AuthorToFollowDto>> GetAuthorsToFollowAsync(Guid userId, int page, int pageSize)
    {
        var publishedBlogs = _dbContext.Blogs
            .Where(blog => blog.Status == BlogStatus.Published && blog.AuthorId != userId);

        var authors = publishedBlogs.Select(blog => blog.AuthorId).Distinct();

        var totalCount = await authors.CountAsync();
        var authorIds = await authors
            .OrderBy(authorId => authorId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var blogs = await publishedBlogs
            .Where(blog => authorIds.Contains(blog.AuthorId))
            .OrderBy(blog => blog.CreatedAt)
            .Select(blog => new { blog.AuthorId, blog.Title })
            .ToListAsync();
        var requests = await _dbContext.FollowRequests
            .Where(request => request.SenderId == userId && authorIds.Contains(request.RecipientId))
            .Select(request => new { request.RecipientId, request.Status })
            .ToListAsync();

        var items = authorIds
            .Select(authorId => new AuthorToFollowDto(
                authorId,
                blogs.Where(blog => blog.AuthorId == authorId).Select(blog => blog.Title).ToList(),
                GetState(requests.Where(request => request.RecipientId == authorId).Select(request => request.Status))))
            .ToList();

        return new PageResult<AuthorToFollowDto>(items, totalCount);
    }

    private static AuthorFollowState GetState(IEnumerable<FollowRequestStatus> statuses)
    {
        var statusList = statuses.ToList();
        if (statusList.Contains(FollowRequestStatus.Accepted))
        {
            return AuthorFollowState.Following;
        }
        if (statusList.Contains(FollowRequestStatus.Pending))
        {
            return AuthorFollowState.RequestSent;
        }
        return AuthorFollowState.NoRequest;
    }
}
