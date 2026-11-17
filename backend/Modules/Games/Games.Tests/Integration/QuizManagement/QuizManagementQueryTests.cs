using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizManagement;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizManagement;

public sealed class QuizManagementQueryTests : BaseIntegrationTest
{
    private const string Url = "/api/games/quizzes";

    public QuizManagementQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_can_list_saved_quizzes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var result = await client.GetFromJsonAsync<PageResult<QuizSummaryDto>>(Url, JsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().BeEquivalentTo(QuizSeed.All.Select(quiz => new QuizSummaryDto(quiz.Id, quiz.Name, quiz.Status)));
        result.TotalCount.Should().Be(QuizSeed.All.Length);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized, false)]
    [InlineData(true, HttpStatusCode.Forbidden, false)]
    [InlineData(false, HttpStatusCode.Unauthorized, true)]
    [InlineData(true, HttpStatusCode.Forbidden, true)]
    public async Task Only_an_administrator_can_read_quizzes(bool signedIn, HttpStatusCode expectedStatus, bool details)
    {
        var client = signedIn ? Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer") : Factory.CreateClient();
        var url = details ? $"{Url}/{QuizSeed.DraftCityWalk.Id}" : Url;

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(expectedStatus);
    }
}
