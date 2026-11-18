namespace Social.Application.BlogReading;

public sealed record BlogCommentDetailDto(
    Guid Id,
    Guid UserId,
    string Text,
    DateTime CreatedAt,
    DateTime LastModifiedAt,
    string? AuthorName,
    bool CanEditOrDelete);
