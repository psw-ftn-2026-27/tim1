using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallengeManagement;

public sealed record SocialChallengeDto(
    Guid Id,
    string Name,
    TimeOnly ActiveFrom,
    TimeOnly ActiveUntil,
    int RequiredParticipants,
    int RadiusMeters,
    int ExperiencePoints,
    string CompletionMessage,
    double Latitude,
    double Longitude,
    SocialChallengeStatus Status);
