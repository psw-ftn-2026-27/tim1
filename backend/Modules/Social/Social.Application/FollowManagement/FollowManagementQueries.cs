using Identity.Contracts;
using Shared.Domain;
using Social.Application.FollowRequests;

namespace Social.Application.FollowManagement;

public sealed class FollowManagementQueries
{
    private readonly IFollowRequestReadRepository _followRequestReadRepository;
    private readonly IIdentityApi _identityApi;

    public FollowManagementQueries(IFollowRequestReadRepository followRequestReadRepository, IIdentityApi identityApi)
    {
        _followRequestReadRepository = followRequestReadRepository;
        _identityApi = identityApi;
    }

    public async Task<PageResult<FollowerDto>> GetFollowersAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var followers = await _followRequestReadRepository.GetFollowersAsync(userId, page, pageSize);

        var displayNames = await GetDisplayNamesAsync(followers.Items.Select(follower => follower.UserId));

        var items = followers.Items
            .Select(follower => follower with { DisplayName = displayNames.GetValueOrDefault(follower.UserId, "") })
            .ToList();

        return new PageResult<FollowerDto>(items, followers.TotalCount);
    }

    public async Task<PageResult<FollowingDto>> GetFollowingAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var following = await _followRequestReadRepository.GetFollowingAsync(userId, page, pageSize);

        var displayNames = await GetDisplayNamesAsync(following.Items.Select(followed => followed.UserId));

        var items = following.Items
            .Select(followed => followed with { DisplayName = displayNames.GetValueOrDefault(followed.UserId, "") })
            .ToList();

        return new PageResult<FollowingDto>(items, following.TotalCount);
    }

    public Task<FollowCountsDto> GetFollowCountsAsync(Guid userId)
    {
        return _followRequestReadRepository.GetFollowCountsAsync(userId);
    }

    private async Task<Dictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> userIds)
    {
        var users = await _identityApi.GetUsersAsync(userIds.Distinct().ToList());
        return users.ToDictionary(user => user.Id, user => user.DisplayName);
    }
}
