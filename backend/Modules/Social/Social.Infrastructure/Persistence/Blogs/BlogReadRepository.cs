using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.BlogReading;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Infrastructure.Persistence.Blogs;

internal sealed class BlogReadRepository : IBlogReadRepository
{
    private readonly SocialDbContext _dbContext;

    public BlogReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<BlogDto?> GetByIdAsync(Guid id)
    {
        return ProjectToDtos(_dbContext.Blogs.Where(blog => blog.Id == id)).FirstOrDefaultAsync();
    }

    public Task<BlogDetailDto?> GetDetailByIdAsync(Guid id, Guid userId)
    {
        return _dbContext.Blogs
            .Where(blog => blog.Id == id)
            .Select(blog => new BlogDetailDto(
                blog.Id,
                blog.AuthorId,
                blog.Title,
                blog.Description,
                blog.CreatedAt,
                blog.LastModifiedAt == DateTime.MinValue ? blog.CreatedAt : blog.LastModifiedAt,
                blog.Images,
                blog.Status,
                blog.Comments
                    .Select(comment => new CommentDto(comment.Id, comment.UserId, comment.Text, comment.CreatedAt))
                    .ToList(),
                blog.Votes.Count(vote => vote.Type == VoteType.Upvote)
                    - blog.Votes.Count(vote => vote.Type == VoteType.Downvote),
                blog.Votes
                    .Where(vote => vote.UserId == userId)
                    .Select(vote => (VoteType?)vote.Type)
                    .FirstOrDefault()))
            .FirstOrDefaultAsync();
    }

    public Task<List<BlogDto>> GetByAuthorAsync(Guid authorId)
    {
        return ProjectToDtos(_dbContext.Blogs.Where(blog => blog.AuthorId == authorId)).ToListAsync();
    }

    public async Task<PageResult<BlogDto>> GetVisibleAsync(int page, int pageSize)
    {
        var visible = _dbContext.Blogs
            .Where(blog => blog.Status == BlogStatus.Published || blog.Status == BlogStatus.Archived)
            .OrderByDescending(blog => blog.CreatedAt);

        var totalCount = await visible.CountAsync();
        var items = await ProjectToDtos(visible)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<BlogDto>(items, totalCount);
    }

    // Blogs that existed before LastModifiedAt was added carry 0001-01-01; they show their
    // creation date as the last modification date, without changing the stored data.
    // The score is the number of upvotes minus the number of downvotes, so a blog without
    // votes scores 0 (P6).
    private static IQueryable<BlogDto> ProjectToDtos(IQueryable<Blog> blogs)
    {
        return blogs
            .Select(blog => new BlogDto(
                blog.Id,
                blog.AuthorId,
                blog.Title,
                blog.Description,
                blog.CreatedAt,
                blog.LastModifiedAt == DateTime.MinValue ? blog.CreatedAt : blog.LastModifiedAt,
                blog.Images,
                blog.Status,
                blog.Comments
                    .Select(comment => new CommentDto(comment.Id, comment.UserId, comment.Text, comment.CreatedAt))
                    .ToList(),
                blog.Votes.Count(vote => vote.Type == VoteType.Upvote)
                    - blog.Votes.Count(vote => vote.Type == VoteType.Downvote)));
    }
}
