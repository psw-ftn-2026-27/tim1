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

    public async Task<List<FollowRequestDto>> GetSentFollowRequestsAsync(Guid userId)
    {
        var requests = await _followRequestReadRepository.GetSentAsync(userId);

        var recipientIds = requests.Select(request => request.RecipientId).Distinct().ToList();
        var recipients = await _identityApi.GetUsersAsync(recipientIds);
        var displayNames = recipients.ToDictionary(recipient => recipient.Id, recipient => recipient.DisplayName);

        return requests
            .Select(request => request with { RecipientDisplayName = displayNames.GetValueOrDefault(request.RecipientId, "") })
            .ToList();
    }

    public async Task<PageResult<UserToFollowDto>> SearchUsersToFollowAsync(Guid userId, string text, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = await _identityApi.SearchUsersAsync(text, page, pageSize);

        var candidateIds = users.Items.Select(user => user.Id).ToList();
        var excludedIds = await _followRequestReadRepository.GetFollowedOrPendingRecipientIdsAsync(userId, candidateIds);

        var items = users.Items
            .Where(user => user.Id != userId && !excludedIds.Contains(user.Id))
            .Select(user => new UserToFollowDto(user.Id, user.DisplayName))
            .ToList();

        return new PageResult<UserToFollowDto>(items, users.TotalCount);
    }
}
