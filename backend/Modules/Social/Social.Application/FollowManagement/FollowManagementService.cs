using Shared.Domain.Exceptions;
using Social.Application.FollowRequests;
using Social.Domain.FollowRequests;

namespace Social.Application.FollowManagement;

public sealed class FollowManagementService
{
    private readonly IFollowRequestRepository _followRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FollowManagementService(IFollowRequestRepository followRequestRepository, IUnitOfWork unitOfWork)
    {
        _followRequestRepository = followRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task RemoveFollowerAsync(Guid userId, Guid requestId)
    {
        var request = await GetRequestAsync(requestId);

        request.RemoveFollower(userId);

        _followRequestRepository.Remove(request);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task StopFollowingAsync(Guid userId, Guid requestId)
    {
        var request = await GetRequestAsync(requestId);

        request.StopFollowing(userId);

        _followRequestRepository.Remove(request);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<FollowRequest> GetRequestAsync(Guid requestId)
    {
        var request = await _followRequestRepository.GetByIdAsync(requestId);
        if (request is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }

        return request;
    }
}
