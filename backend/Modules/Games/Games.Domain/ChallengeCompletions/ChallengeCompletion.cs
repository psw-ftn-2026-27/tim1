using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.ChallengeCompletions;

public sealed class ChallengeCompletion : AggregateRoot
{
    public Guid PlayerId { get; private set; }
    public Guid ChallengeId { get; private set; }
    public ChallengeType ChallengeType { get; private set; }
    public DateTime CompletedAt { get; private set; }
    public int ExperiencePointsAwarded { get; private set; }

    private ChallengeCompletion() { }

    public ChallengeCompletion(Guid playerId, Guid challengeId, ChallengeType challengeType, DateTime completedAt,
        int experiencePointsAwarded)
        : base(Guid.NewGuid())
    {
        if (experiencePointsAwarded < 0)
        {
            throw new DomainException("Awarded experience points cannot be negative.");
        }

        PlayerId = playerId;
        ChallengeId = challengeId;
        ChallengeType = challengeType;
        CompletedAt = completedAt;
        ExperiencePointsAwarded = experiencePointsAwarded;
    }
}
