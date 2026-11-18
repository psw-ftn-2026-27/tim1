namespace Social.Application.ClubInviting;

public sealed record SentInvitationDto(Guid UserId, string UserDisplayName, DateTime CreatedAt);
