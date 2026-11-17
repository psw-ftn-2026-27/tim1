namespace Social.Application.FollowManagement;

public sealed record FollowingDto(Guid Id, Guid UserId, string DisplayName);
