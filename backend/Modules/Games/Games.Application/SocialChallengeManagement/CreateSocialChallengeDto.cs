namespace Games.Application.SocialChallengeManagement;

public sealed record CreateSocialChallengeDto(
    string Name,
    TimeOnly ActiveFrom,
    TimeOnly ActiveUntil,
    int RequiredParticipants,
    int RadiusMeters,
    int ExperiencePoints,
    string CompletionMessage,
    double? Latitude,
    double? Longitude);
