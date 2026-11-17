using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.FollowManagement;
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

    public async Task<PageResult<FollowerDto>> GetFollowersAsync(Guid userId, int page, int pageSize)
    {
        var followers = _dbContext.FollowRequests
            .Where(request => request.RecipientId == userId && request.Status == FollowRequestStatus.Accepted);

        var totalCount = await followers.CountAsync();
        var items = await followers
            .OrderByDescending(request => request.CreatedAt)
            .ThenBy(request => request.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(request => new FollowerDto(
                request.Id,
                request.SenderId,
                "",
                _dbContext.FollowRequests.Any(back => back.SenderId == userId
                    && back.RecipientId == request.SenderId
                    && back.Status == FollowRequestStatus.Accepted),
                _dbContext.FollowRequests.Any(back => back.SenderId == userId
                    && back.RecipientId == request.SenderId
                    && back.Status == FollowRequestStatus.Pending)))
            .ToListAsync();

        return new PageResult<FollowerDto>(items, totalCount);
    }

    public async Task<PageResult<FollowingDto>> GetFollowingAsync(Guid userId, int page, int pageSize)
    {
        var following = _dbContext.FollowRequests
            .Where(request => request.SenderId == userId && request.Status == FollowRequestStatus.Accepted);

        var totalCount = await following.CountAsync();
        var items = await following
            .OrderByDescending(request => request.CreatedAt)
            .ThenBy(request => request.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(request => new FollowingDto(request.Id, request.RecipientId, ""))
            .ToListAsync();

        return new PageResult<FollowingDto>(items, totalCount);
    }

    public async Task<FollowCountsDto> GetFollowCountsAsync(Guid userId)
    {
        var followerCount = await _dbContext.FollowRequests
            .CountAsync(request => request.RecipientId == userId && request.Status == FollowRequestStatus.Accepted);
        var followingCount = await _dbContext.FollowRequests
            .CountAsync(request => request.SenderId == userId && request.Status == FollowRequestStatus.Accepted);

        return new FollowCountsDto(followerCount, followingCount);
    }
}
