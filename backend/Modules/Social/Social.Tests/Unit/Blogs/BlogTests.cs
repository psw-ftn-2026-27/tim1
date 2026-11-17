using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Blogs;
using Xunit;

namespace Social.Tests.Unit.Blogs;

public class BlogTests
{
    private static Blog CreateBlog() =>
        new(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", [Guid.NewGuid()]);

    private static Blog CreatePublishedBlog()
    {
        var blog = CreateBlog();
        blog.Publish();
        return blog;
    }

    private static Blog CreateArchivedBlog()
    {
        var blog = CreatePublishedBlog();
        blog.Archive();
        return blog;
    }

    [Fact]
    public void New_blog_starts_as_a_draft()
    {
        var blog = CreateBlog();

        blog.Status.Should().Be(BlogStatus.Draft);
        blog.AuthorId.Should().Be(WellKnownUsers.Explorer);
        blog.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        blog.Comments.Should().BeEmpty();
    }

    [Fact]
    public void New_blog_has_the_same_last_modified_date_as_creation_date()
    {
        var blog = CreateBlog();

        blog.LastModifiedAt.Should().Be(blog.CreatedAt);
    }

    [Fact]
    public void New_blog_without_images_has_no_images()
    {
        var blog = new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", null);

        blog.Images.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_blog_requires_a_title(string title)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, title, "Opis obilaska tvrđave.", []);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_blog_requires_a_description(string description)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", description, []);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_blog_can_have_20_images()
    {
        var images = Enumerable.Range(0, 20).Select(_ => Guid.NewGuid()).ToList();

        var blog = new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", images);

        blog.Images.Should().Equal(images);
    }

    [Fact]
    public void New_blog_cannot_have_more_than_20_images()
    {
        var images = Enumerable.Range(0, 21).Select(_ => Guid.NewGuid()).ToList();

        var creation = () => new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", images);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_blog_is_published()
    {
        var blog = CreateBlog();

        blog.Publish();

        blog.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public void Published_blog_cannot_be_published_again()
    {
        var blog = CreatePublishedBlog();

        var publishing = () => blog.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_blog_cannot_be_archived()
    {
        var blog = CreateBlog();

        var archiving = () => blog.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_be_archived_again()
    {
        var blog = CreateArchivedBlog();

        var archiving = () => blog.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_content_cannot_be_updated()
    {
        var blog = CreateArchivedBlog();

        var update = () => blog.UpdateContent("Novi naslov", "Novi opis obilaska tvrđave.", [Guid.NewGuid()]);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Updated_blog_gets_a_new_last_modified_date_and_keeps_its_creation_date()
    {
        var blog = CreateBlog();
        var createdAt = blog.CreatedAt;
        Thread.Sleep(TimeSpan.FromMilliseconds(10));

        blog.UpdateContent("Novi naslov", "Novi opis obilaska tvrđave.", []);

        blog.CreatedAt.Should().Be(createdAt);
        blog.LastModifiedAt.Should().BeAfter(createdAt);
    }

    [Fact]
    public void Published_blog_content_is_updated()
    {
        var blog = CreatePublishedBlog();
        var images = new List<Guid> { Guid.NewGuid() };

        blog.UpdateContent("Novi naslov", "Novi opis obilaska tvrđave.", images);

        blog.Title.Should().Be("Novi naslov");
        blog.Description.Should().Be("Novi opis obilaska tvrđave.");
        blog.Images.Should().Equal(images);
        blog.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public void User_comments_on_a_published_blog()
    {
        var blog = CreatePublishedBlog();

        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        blog.Comments.Should().ContainSingle(comment =>
            comment.UserId == WellKnownUsers.Administrator && comment.Text == "Odlična tura!");
    }

    [Fact]
    public void Archived_blog_cannot_be_commented_on()
    {
        var blog = CreateArchivedBlog();

        var addition = () => blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        addition.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Comment_requires_text(string text)
    {
        var blog = CreatePublishedBlog();

        var addition = () => blog.AddComment(WellKnownUsers.Administrator, text);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_edits_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.");

        blog.Comments.Single().Text.Should().Be("Ipak prosečna tura.");
    }

    [Fact]
    public void User_cannot_edit_another_users_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var update = () => blog.UpdateComment(comment.Id, WellKnownUsers.Explorer, "Izmena tuđeg komentara.");

        update.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_deletes_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.DeleteComment(comment.Id, WellKnownUsers.Administrator);

        blog.Comments.Should().BeEmpty();
    }

}
