using Shared.Domain;

namespace Social.Domain.Clubs;

public sealed class ClubInvitation : Entity
{
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ClubInvitation() { }

    public ClubInvitation(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }
}
