using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.Blogs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogAuthoring;

public class BlogAuthoringQueryTests : BaseIntegrationTest
{
    public BlogAuthoringQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_lists_only_their_own_blogs()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var blogs = await client.GetFromJsonAsync<List<BlogDto>>("/api/social/blogs/mine", JsonOptions);

        blogs.Should().HaveCount(BlogSeed.All.Count(blog => blog.AuthorId == WellKnownUsers.Explorer));
        blogs.Should().OnlyContain(blog => blog.AuthorId == WellKnownUsers.Explorer);
        blogs.Should().NotContain(blog => blog.Id == BlogSeed.SecondExplorersDiary.Id);
    }

    [Fact]
    public async Task Author_gets_an_image_of_their_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync(
            $"/api/social/blogs/mine/{BlogSeed.FortressImpressions.Id}/images/{ImageSeed.OldFortressPhotoOnDraft.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/webp");
        (await response.Content.ReadAsByteArrayAsync()).Should().Equal(SocialApiFactory.SeededImageContent);
    }

    [Fact]
    public async Task Another_user_does_not_get_an_image_of_someone_elses_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(
            $"/api/social/blogs/mine/{BlogSeed.FortressImpressions.Id}/images/{ImageSeed.OldFortressPhotoOnDraft.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
