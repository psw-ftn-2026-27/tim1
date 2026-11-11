using Shared.Domain.Exceptions;
using Social.Application.FollowRequests;
using Social.Domain.FollowRequests;

namespace Social.Application.FollowRequesting;

public sealed class FollowRequestingService
{
    private readonly IFollowRequestRepository _followRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FollowRequestingService(IFollowRequestRepository followRequestRepository, IUnitOfWork unitOfWork)
    {
        _followRequestRepository = followRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SendFollowRequestAsync(Guid userId, Guid recipientId)
    {
        var existingRequests = await _followRequestRepository.GetBetweenAsync(userId, recipientId);

        var request = FollowRequestSending.Send(userId, recipientId, existingRequests);

        _followRequestRepository.Add(request);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteFollowRequestAsync(Guid userId, Guid requestId)
    {
        var request = await _followRequestRepository.GetByIdAsync(requestId);
        if (request is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }

        request.Delete(userId);

        _followRequestRepository.Remove(request);
        await _unitOfWork.SaveChangesAsync();
    }
}
