namespace Social.Application.FollowRequesting;

public sealed record AuthorToFollowDto(Guid AuthorId, List<string> BlogTitles, AuthorFollowState State);
