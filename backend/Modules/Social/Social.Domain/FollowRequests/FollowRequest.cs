using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.FollowRequests;

public sealed class FollowRequest : AggregateRoot
{
    public Guid SenderId { get; private set; }
    public Guid RecipientId { get; private set; }
    public FollowRequestStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private FollowRequest() { }

    public FollowRequest(Guid senderId, Guid recipientId)
        : base(Guid.NewGuid())
    {
        if (senderId == recipientId)
        {
            throw new DomainException("A user cannot send a follow request to themselves.");
        }

        SenderId = senderId;
        RecipientId = recipientId;
        Status = FollowRequestStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Accept()
    {
        EnsurePending("accepted");

        Status = FollowRequestStatus.Accepted;
    }

    public void Reject()
    {
        EnsurePending("rejected");

        Status = FollowRequestStatus.Rejected;
    }

    public void Delete(Guid userId)
    {
        if (SenderId != userId)
        {
            throw new DomainException("Only the sender can delete a follow request.");
        }

        EnsurePending("deleted");
    }

    private void EnsurePending(string action)
    {
        if (Status != FollowRequestStatus.Pending)
        {
            throw new DomainException($"Only a pending follow request can be {action}.");
        }
    }
}
