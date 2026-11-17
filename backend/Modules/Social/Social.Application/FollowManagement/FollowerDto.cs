namespace Social.Application.FollowManagement;

public sealed record FollowerDto(Guid Id, Guid UserId, string DisplayName, bool IsFollowedByUser, bool HasPendingRequest);
