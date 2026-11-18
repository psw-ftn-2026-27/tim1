using Shared.Domain;

namespace Social.Domain.Clubs;

public sealed class MembershipRequest : Entity
{
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private MembershipRequest() { }

    public MembershipRequest(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }
}
