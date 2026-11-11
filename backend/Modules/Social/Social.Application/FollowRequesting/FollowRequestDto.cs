using Social.Domain.FollowRequests;

namespace Social.Application.FollowRequesting;

public sealed record FollowRequestDto(Guid Id, Guid RecipientId, FollowRequestStatus Status, DateTime CreatedAt);
