using Shared.Domain;
using Social.Application.FollowRequests;

namespace Social.Application.FollowRequesting;

public sealed class FollowRequestingQueries
{
    private readonly IFollowRequestReadRepository _followRequestReadRepository;

    public FollowRequestingQueries(IFollowRequestReadRepository followRequestReadRepository)
    {
        _followRequestReadRepository = followRequestReadRepository;
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
