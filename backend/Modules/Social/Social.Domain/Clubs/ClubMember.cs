using Shared.Domain;

namespace Social.Domain.Clubs;

public sealed class ClubMember : Entity
{
    public Guid UserId { get; private set; }
    public ClubRole Role { get; private set; }

    private ClubMember() { }

    public ClubMember(Guid userId, ClubRole role) : base(Guid.NewGuid())
    {
        UserId = userId;
        Role = role;
    }
}
