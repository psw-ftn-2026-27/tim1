using Social.Domain.FollowRequests;

namespace Social.Application.FollowRequests;

public interface IFollowRequestRepository
{
    Task<FollowRequest?> GetByIdAsync(Guid id);
    Task<List<FollowRequest>> GetBetweenAsync(Guid firstUserId, Guid secondUserId);
    void Add(FollowRequest request);
    void Remove(FollowRequest request);
}
