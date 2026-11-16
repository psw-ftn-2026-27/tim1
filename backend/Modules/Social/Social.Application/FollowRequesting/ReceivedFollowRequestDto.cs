using Social.Domain.FollowRequests;

namespace Social.Application.FollowRequesting;

public sealed record ReceivedFollowRequestDto(
    Guid Id,
    Guid SenderId,
    string SenderDisplayName,
    FollowRequestStatus Status,
    DateTime CreatedAt);
