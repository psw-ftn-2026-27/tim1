using Shared.Domain.Exceptions;
using Social.Application.Blogs;

namespace Social.Application.BlogVoting;

public sealed class BlogVotingService
{
    private readonly IBlogRepository _blogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BlogVotingService(IBlogRepository blogRepository, IUnitOfWork unitOfWork)
    {
        _blogRepository = blogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task VoteAsync(Guid blogId, Guid userId, VoteDto dto)
    {
        var blog = await _blogRepository.GetByIdAsync(blogId);
        if (blog is null)
        {
            throw new NotFoundException($"Blog {blogId} does not exist.");
        }

        blog.Vote(userId, dto.Type);
        await _unitOfWork.SaveChangesAsync();
    }
}
