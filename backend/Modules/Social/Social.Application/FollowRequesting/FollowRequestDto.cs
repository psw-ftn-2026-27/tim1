using Social.Domain.FollowRequests;

namespace Social.Application.FollowRequesting;

public sealed record FollowRequestDto(Guid Id, Guid RecipientId, string RecipientDisplayName, FollowRequestStatus Status, DateTime CreatedAt);
