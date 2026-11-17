using Shared.Domain;
using Social.Application.FollowManagement;
using Social.Application.FollowRequesting;

namespace Social.Application.FollowRequests;

public interface IFollowRequestReadRepository
{
    Task<List<FollowRequestDto>> GetSentAsync(Guid senderId);
    Task<PageResult<ReceivedFollowRequestDto>> GetReceivedAsync(Guid recipientId, int page, int pageSize);
    Task<int> CountPendingReceivedAsync(Guid recipientId);
    Task<List<Guid>> GetFollowedOrPendingRecipientIdsAsync(Guid senderId, IReadOnlyCollection<Guid> recipientIds);
    Task<PageResult<FollowerDto>> GetFollowersAsync(Guid userId, int page, int pageSize);
    Task<PageResult<FollowingDto>> GetFollowingAsync(Guid userId, int page, int pageSize);
    Task<FollowCountsDto> GetFollowCountsAsync(Guid userId);
}
