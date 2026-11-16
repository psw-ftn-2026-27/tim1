using Identity.Contracts;
using Shared.Domain;
using Social.Application.FollowRequests;

namespace Social.Application.FollowRequesting;

public sealed class FollowRequestingQueries
{
    private readonly IFollowRequestReadRepository _followRequestReadRepository;

    private readonly IIdentityApi _identityApi;

    public FollowRequestingQueries(IFollowRequestReadRepository followRequestReadRepository, IIdentityApi identityApi)
    {
        _followRequestReadRepository = followRequestReadRepository;
        _identityApi = identityApi;
    }

    public async Task<PageResult<ReceivedFollowRequestDto>> GetReceivedFollowRequestsAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var requests = await _followRequestReadRepository.GetReceivedAsync(userId, page, pageSize);

        var senderIds = requests.Items.Select(request => request.SenderId).Distinct().ToList();
        var senders = await _identityApi.GetUsersAsync(senderIds);
        var displayNames = senders.ToDictionary(sender => sender.Id, sender => sender.DisplayName);

        var items = requests.Items
            .Select(request => request with { SenderDisplayName = displayNames.GetValueOrDefault(request.SenderId, "") })
            .ToList();

        return new PageResult<ReceivedFollowRequestDto>(items, requests.TotalCount);
    }

    public Task<int> GetPendingReceivedCountAsync(Guid userId)
    {
        return _followRequestReadRepository.CountPendingReceivedAsync(userId);
    }

    public Task<List<FollowRequestDto>> GetSentFollowRequestsAsync(Guid userId)
    {
        return _followRequestReadRepository.GetSentAsync(userId);
    }

    public Task<PageResult<AuthorToFollowDto>> GetAuthorsToFollowAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _followRequestReadRepository.GetAuthorsToFollowAsync(userId, page, pageSize);
    }
}
