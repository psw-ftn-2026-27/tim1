namespace Social.Application.ClubInviting;

public sealed record MyInvitationDto(Guid ClubId, string ClubName, DateTime CreatedAt);
