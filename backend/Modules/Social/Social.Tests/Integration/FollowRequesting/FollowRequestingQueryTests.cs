using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.FollowRequesting;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowRequesting;

public class FollowRequestingQueryTests : BaseIntegrationTest
{
    public FollowRequestingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_lists_only_the_follow_requests_they_sent()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var requests = await client.GetFromJsonAsync<List<FollowRequestDto>>("/api/social/follow-requests/sent", JsonOptions);

        requests.Should().HaveCount(FollowRequestSeed.All.Count(request => request.SenderId == WellKnownUsers.Explorer));
        requests.Should().Contain(request => request.Id == FollowRequestSeed.ExplorerPendingToSecondExplorer.Id);
        requests.Should().NotContain(request => request.Id == FollowRequestSeed.AdministratorPendingToExplorer.Id);
    }

    [Fact]
    public async Task Authors_to_follow_exclude_the_explorer_and_show_the_request_state()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var authors = await client.GetFromJsonAsync<PageResult<AuthorToFollowDto>>("/api/social/follow-requests/authors", JsonOptions);

        authors!.Items.Should().NotContain(author => author.AuthorId == WellKnownUsers.Explorer);
        var secondExplorer = authors.Items.Single(author => author.AuthorId == WellKnownUsers.SecondExplorer);
        secondExplorer.State.Should().Be(AuthorFollowState.RequestSent);
        secondExplorer.BlogTitles.Should().Contain(BlogSeed.PublishedSecondExplorersTrail.Title);
    }

    [Fact]
    public async Task Anonymous_user_cannot_list_sent_follow_requests()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/social/follow-requests/sent");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
