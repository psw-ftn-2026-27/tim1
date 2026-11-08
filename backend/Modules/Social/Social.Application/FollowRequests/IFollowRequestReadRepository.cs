using Shared.Domain;
using Social.Application.FollowRequesting;

namespace Social.Application.FollowRequests;

public interface IFollowRequestReadRepository
{
    Task<List<FollowRequestDto>> GetSentAsync(Guid senderId);
    Task<PageResult<AuthorToFollowDto>> GetAuthorsToFollowAsync(Guid userId, int page, int pageSize);
}
