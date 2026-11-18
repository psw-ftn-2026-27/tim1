using Social.Domain.Blogs;

namespace Social.Application.BlogReading;

public sealed record BlogDetailDto(
    Guid Id,
    Guid AuthorId,
    string Title,
    string Description,
    DateTime CreatedAt,
    DateTime LastModifiedAt,
    List<Guid> Images,
    BlogStatus Status,
    List<BlogCommentDetailDto> Comments,
    int Score,
    VoteType? MyVote);
