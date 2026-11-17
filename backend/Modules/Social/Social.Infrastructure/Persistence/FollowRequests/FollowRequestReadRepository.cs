using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.FollowRequesting;
using Social.Application.FollowRequests;
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
            .Select(request => new FollowRequestDto(request.Id, request.RecipientId, "", request.Status, request.CreatedAt))
            .ToListAsync();
    }

    public async Task<PageResult<ReceivedFollowRequestDto>> GetReceivedAsync(Guid recipientId, int page, int pageSize)
    {
        var received = _dbContext.FollowRequests.Where(request => request.RecipientId == recipientId);

        var totalCount = await received.CountAsync();
        var items = await received
            .OrderByDescending(request => request.CreatedAt)
            .ThenBy(request => request.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(request => new ReceivedFollowRequestDto(request.Id, request.SenderId, "", request.Status, request.CreatedAt))
            .ToListAsync();

        return new PageResult<ReceivedFollowRequestDto>(items, totalCount);
    }

    public Task<int> CountPendingReceivedAsync(Guid recipientId)
    {
        return _dbContext.FollowRequests
            .CountAsync(request => request.RecipientId == recipientId && request.Status == FollowRequestStatus.Pending);
    }

    public Task<List<Guid>> GetFollowedOrPendingRecipientIdsAsync(Guid senderId, IReadOnlyCollection<Guid> recipientIds)
    {
        return _dbContext.FollowRequests
            .Where(request => request.SenderId == senderId
                && recipientIds.Contains(request.RecipientId)
                && (request.Status == FollowRequestStatus.Accepted || request.Status == FollowRequestStatus.Pending))
            .Select(request => request.RecipientId)
            .Distinct()
            .ToListAsync();
    }
}
