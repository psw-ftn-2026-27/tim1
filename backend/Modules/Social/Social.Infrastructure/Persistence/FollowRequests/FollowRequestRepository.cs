using Microsoft.EntityFrameworkCore;
using Social.Application.FollowRequests;
using Social.Domain.FollowRequests;

namespace Social.Infrastructure.Persistence.FollowRequests;

internal sealed class FollowRequestRepository : IFollowRequestRepository
{
    private readonly SocialDbContext _dbContext;

    public FollowRequestRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<FollowRequest?> GetByIdAsync(Guid id)
    {
        return _dbContext.FollowRequests.FirstOrDefaultAsync(request => request.Id == id);
    }

    public Task<List<FollowRequest>> GetBetweenAsync(Guid firstUserId, Guid secondUserId)
    {
        return _dbContext.FollowRequests
            .Where(request =>
                (request.SenderId == firstUserId && request.RecipientId == secondUserId)
                || (request.SenderId == secondUserId && request.RecipientId == firstUserId))
            .ToListAsync();
    }

    public void Add(FollowRequest request)
    {
        _dbContext.FollowRequests.Add(request);
    }

    public void Remove(FollowRequest request)
    {
        _dbContext.FollowRequests.Remove(request);
    }
}
