using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Games.Application.ChallengeOverview;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Microsoft.EntityFrameworkCore;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.ChallengeOverview;

public sealed class ChallengeOverviewQueryTests : BaseIntegrationTest
{
    private const string Url = "/api/games/challenges";

    public ChallengeOverviewQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_both_challenge_types_in_all_states()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var expected = QuizSeed.All.Select(quiz => new ChallengeOverviewItemDto(
                quiz.Id, quiz.Name, "Quiz", quiz.Status.ToString(), quiz.CreatedAt))
            .Concat(SocialChallengeSeed.All.Select(challenge => new ChallengeOverviewItemDto(
                challenge.Id, challenge.Name, "SocialChallenge", challenge.Status.ToString(), challenge.CreatedAt)));

        var result = await client.GetFromJsonAsync<ChallengeOverviewDto>(Url, JsonOptions);

        result!.HasChallenges.Should().BeTrue();
        result.Page.TotalCount.Should().Be(QuizSeed.All.Length + SocialChallengeSeed.All.Length);
        result.Page.Items.Should().BeEquivalentTo(expected, options => options
            .Using<DateTime>(date => date.Subject.Should().BeCloseTo(date.Expectation, TimeSpan.FromTicks(10)))
            .WhenTypeIs<DateTime>());
    }

    [Fact]
    public async Task A_legacy_social_challenge_keeps_an_unknown_creation_date()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var context = Factory.CreateContext<GamesDbContext>();
        var challenge = SocialChallengeSeed.DraftQuayWalk;
        var database = context.Database.GetDbConnection().Database;
        if (database != "explorer-test-games")
        {
            throw new InvalidOperationException("The legacy fixture may only write to the Games test database.");
        }

        // Simulate a pre-migration row after the fixture seeds it through the normal constructor.
        var updated = await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE games.\"SocialChallenges\" SET \"CreatedAt\" = NULL WHERE \"Id\" = {challenge.Id}");
        updated.Should().Be(1);

        var result = await client.GetFromJsonAsync<JsonElement>(Url, JsonOptions);

        var item = result.GetProperty("page").GetProperty("items").EnumerateArray()
            .Single(item => item.GetProperty("id").GetGuid() == challenge.Id);
        item.GetProperty("createdAt").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Search_and_filters_together_select_only_matching_challenges()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var wanted = new Quiz("City walk", "Read the signs.", 0, []);
        var otherName = new Quiz("Museum visit", "Read the signs.", 0, []);
        var otherStatus = new Quiz("City museum", "Read the signs.", 0,
            [new QuizQuestion("Which sign?", [new QuizAnswer("One", true), new QuizAnswer("Two", false)])]);
        otherStatus.Publish();
        var otherType = CreateSocialChallenge("City walk");
        Factory.Reseed<GamesDbContext>([wanted, otherName, otherStatus, otherType]);

        var result = await client.GetFromJsonAsync<ChallengeOverviewDto>(
            $"{Url}?name=%20CITY%20&type=Quiz&status=Draft", JsonOptions);

        result!.Page.TotalCount.Should().Be(1);
        result.Page.Items.Should().ContainSingle().Which.Id.Should().Be(wanted.Id);
    }

    [Fact]
    public async Task Matching_challenges_with_the_same_name_are_paged_in_stable_order()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = new Quiz("City walk", "Read the signs.", 0, []);
        var socialChallenge = CreateSocialChallenge("City walk");
        Factory.Reseed<GamesDbContext>([socialChallenge, new Quiz("Absent", "Read the signs.", 0, []), quiz]);
        const string query = "?name=city&status=Draft&pageSize=1";

        var first = await client.GetFromJsonAsync<ChallengeOverviewDto>($"{Url}{query}&page=1", JsonOptions);
        var second = await client.GetFromJsonAsync<ChallengeOverviewDto>($"{Url}{query}&page=2", JsonOptions);

        first!.Page.TotalCount.Should().Be(2);
        second!.Page.TotalCount.Should().Be(2);
        first.Page.Items.Should().ContainSingle().Which.Id.Should().Be(quiz.Id);
        second.Page.Items.Should().ContainSingle().Which.Id.Should().Be(socialChallenge.Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Empty_results_report_whether_any_challenge_exists(bool hasChallenges)
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        if (!hasChallenges)
        {
            Factory.Reseed<GamesDbContext>([]);
        }

        var result = await client.GetFromJsonAsync<ChallengeOverviewDto>($"{Url}?name=unmatched-name", JsonOptions);

        result!.HasChallenges.Should().Be(hasChallenges);
        result.Page.TotalCount.Should().Be(0);
        result.Page.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task Only_an_administrator_can_see_the_challenge_overview(bool signedIn, HttpStatusCode expectedStatus)
    {
        using var client = signedIn ? Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer") : Factory.CreateClient();

        var response = await client.GetAsync(Url);

        response.StatusCode.Should().Be(expectedStatus);
    }

    private static SocialChallenge CreateSocialChallenge(string name) =>
        new(name, new TimeOnly(10, 0), new TimeOnly(11, 0), 2, 10, 5, "Well done!", 45.25, 19.85);
}
