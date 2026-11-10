using Shared.Domain.Exceptions;

namespace Social.Domain.FollowRequests;

public static class FollowRequestSending
{
    public static FollowRequest Send(Guid senderId, Guid recipientId, IReadOnlyList<FollowRequest> existingRequests)
    {
        var request = new FollowRequest(senderId, recipientId);

        var alreadyRequestedOrFollowing = existingRequests.Any(existing =>
            existing.SenderId == senderId
            && existing.RecipientId == recipientId
            && existing.Status != FollowRequestStatus.Rejected);
        if (alreadyRequestedOrFollowing)
        {
            throw new DomainException("A pending or accepted follow request to this user already exists.");
        }

        var recipientAlreadyFollowsSender = existingRequests.Any(existing =>
            existing.SenderId == recipientId
            && existing.RecipientId == senderId
            && existing.Status == FollowRequestStatus.Accepted);
        if (recipientAlreadyFollowsSender)
        {
            request.Accept();
        }

        return request;
    }
}
