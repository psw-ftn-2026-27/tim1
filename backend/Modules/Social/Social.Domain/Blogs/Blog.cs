using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Blogs;

public sealed class Blog : AggregateRoot
{
    public const int MaxImages = 20;

    public Guid AuthorId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastModifiedAt { get; private set; }
    public List<Guid> Images { get; private set; }
    public BlogStatus Status { get; private set; }
    private readonly List<Comment> _comments = [];
    public IReadOnlyList<Comment> Comments => _comments;
    private readonly List<Vote> _votes = [];
    public IReadOnlyList<Vote> Votes => _votes;

    private Blog()
    {
        Title = null!;
        Description = null!;
        Images = null!;
    }

    public Blog(Guid authorId, string title, string description, List<Guid>? images)
        : base(Guid.NewGuid())
    {
        EnsureValidContent(title, description, images);

        AuthorId = authorId;
        Title = title;
        Description = description;
        var now = DateTime.UtcNow;
        CreatedAt = now;
        LastModifiedAt = now;
        Images = images ?? [];
        Status = BlogStatus.Draft;
    }

    public void UpdateContent(string title, string description, List<Guid>? images)
    {
        if (Status == BlogStatus.Archived)
        {
            throw new DomainException("An archived blog cannot be changed.");
        }
        EnsureValidContent(title, description, images);

        Title = title;
        Description = description;
        Images = images ?? [];
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (Status != BlogStatus.Draft)
        {
            throw new DomainException("Only a draft blog can be published.");
        }

        Status = BlogStatus.Published;
    }

    public void Archive()
    {
        if (Status != BlogStatus.Published)
        {
            throw new DomainException("Only a published blog can be archived.");
        }

        Status = BlogStatus.Archived;
    }

    public void AddComment(Guid userId, string text)
    {
        EnsurePublished();

        _comments.Add(new Comment(userId, text));
    }

    public void UpdateComment(Guid commentId, Guid userId, string text, DateTime now)
    {
        EnsurePublished();
        var comment = GetOwnComment(commentId, userId);
        EnsureEditable(comment, now);

        comment.Update(text, now);
    }

    public void DeleteComment(Guid commentId, Guid userId, DateTime now)
    {
        EnsurePublished();
        var comment = GetOwnComment(commentId, userId);
        EnsureEditable(comment, now);

        _comments.Remove(comment);
    }

    public void Vote(Guid userId, VoteType type)
    {
        EnsurePublished();

        var vote = _votes.FirstOrDefault(vote => vote.UserId == userId);
        if (vote is null)
        {
            _votes.Add(new Vote(userId, type));
        }
        else if (vote.Type == type)
        {
            _votes.Remove(vote);
        }
        else
        {
            vote.ChangeType(type);
        }
    }

    private static void EnsureValidContent(string title, string description, List<Guid>? images)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("A blog requires a title.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A blog requires a description.");
        }
        if (images is not null && images.Count > MaxImages)
        {
            throw new DomainException($"A blog can have at most {MaxImages} images.");
        }
    }

    private void EnsurePublished()
    {
        if (Status != BlogStatus.Published)
        {
            throw new DomainException("Comments and votes are allowed only on a published blog.");
        }
    }

    private static void EnsureEditable(Comment comment, DateTime now)
    {
        if (!comment.IsEditableAt(now))
        {
            throw new DomainException("A comment can be changed only within 15 minutes of its creation.");
        }
    }

    private Comment GetOwnComment(Guid commentId, Guid userId)
    {
        var comment = _comments.FirstOrDefault(comment => comment.Id == commentId && comment.UserId == userId);
        if (comment is null)
        {
            throw new NotFoundException($"Comment {commentId} does not exist.");
        }
        return comment;
    }
}
