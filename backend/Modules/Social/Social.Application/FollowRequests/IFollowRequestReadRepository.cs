using Shared.Domain;
using Social.Application.FollowRequesting;

namespace Social.Application.FollowRequests;

public interface IFollowRequestReadRepository
{
    Task<List<FollowRequestDto>> GetSentAsync(Guid senderId);
    Task<PageResult<ReceivedFollowRequestDto>> GetReceivedAsync(Guid recipientId, int page, int pageSize);
    Task<int> CountPendingReceivedAsync(Guid recipientId);
    Task<PageResult<AuthorToFollowDto>> GetAuthorsToFollowAsync(Guid userId, int page, int pageSize);
}
