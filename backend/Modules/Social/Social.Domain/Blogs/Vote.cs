using Shared.Domain;

namespace Social.Domain.Blogs;

public sealed class Vote : Entity
{
    public Guid UserId { get; private set; }
    public VoteType Type { get; private set; }
    public DateTime VotedAt { get; private set; }

    private Vote() { }

    public Vote(Guid userId, VoteType type) : base(Guid.NewGuid())
    {
        UserId = userId;
        Type = type;
        VotedAt = DateTime.UtcNow;
    }

    public void ChangeType(VoteType type)
    {
        Type = type;
        VotedAt = DateTime.UtcNow;
    }
}
