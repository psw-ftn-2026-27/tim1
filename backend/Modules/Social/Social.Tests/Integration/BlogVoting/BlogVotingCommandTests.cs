using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.BlogVoting;
using Social.Domain.Blogs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogVoting;

public class BlogVotingCommandTests : BaseIntegrationTest
{
    public BlogVotingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_votes_on_a_published_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new VoteDto(VoteType.Upvote);

        var response = await client.PostAsJsonAsync($"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/votes",
            request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.PublishedRiverbank.Id);
        stored.Votes.Should().ContainSingle(vote =>
            vote.UserId == WellKnownUsers.Administrator && vote.Type == VoteType.Upvote);
    }

    [Fact]
    public async Task Vote_on_an_archived_blog_is_rejected_and_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new VoteDto(VoteType.Upvote);

        var response = await client.PostAsJsonAsync(
            $"/api/social/blogs/{BlogSeed.CommentedArchivedFestival.Id}/votes", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.CommentedArchivedFestival.Id);
        stored.Votes.Should().NotContain(vote => vote.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Vote_on_an_unknown_blog_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        int voteCountBefore;
        using (var arrangeContext = Factory.CreateContext<SocialDbContext>())
        {
            voteCountBefore = arrangeContext.Blogs.SelectMany(blog => blog.Votes).Count();
        }
        var request = new VoteDto(VoteType.Upvote);

        var response = await client.PostAsJsonAsync($"/api/social/blogs/{Guid.NewGuid()}/votes", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.SelectMany(blog => blog.Votes).Count().Should().Be(voteCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_vote()
    {
        var client = Factory.CreateClient();
        var request = new VoteDto(VoteType.Upvote);

        var response = await client.PostAsJsonAsync($"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/votes",
            request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
