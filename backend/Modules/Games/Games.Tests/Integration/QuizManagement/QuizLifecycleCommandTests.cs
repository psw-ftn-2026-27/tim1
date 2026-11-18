using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Microsoft.AspNetCore.Mvc;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizManagement;

public sealed class QuizLifecycleCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/games/quizzes";

    public QuizLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    [Theory]
    [InlineData("publish", QuizStatus.Draft, QuizStatus.Published)]
    [InlineData("retire", QuizStatus.Published, QuizStatus.Retired)]
    [InlineData("archive", QuizStatus.Retired, QuizStatus.Archived)]
    public async Task Administrator_can_advance_a_quiz_to_the_next_state(
        string command, QuizStatus startingStatus, QuizStatus nextStatus)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = GetSeed(startingStatus);

        var response = await client.PostAsync($"{Url}/{quiz.Id}/{command}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.Quizzes.Single(stored => stored.Id == quiz.Id).Status.Should().Be(nextStatus);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Archived)]
    public async Task Administrator_can_delete_a_draft_or_archived_quiz(QuizStatus status)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = GetSeed(status);

        var response = await client.DeleteAsync($"{Url}/{quiz.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.Quizzes.Any(stored => stored.Id == quiz.Id).Should().BeFalse();
    }

    [Fact]
    public async Task An_incomplete_quiz_cannot_be_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = QuizSeed.DraftCityWalk;

        var response = await client.PostAsync($"{Url}/{quiz.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        problem!.Title.Should().Contain("answer");
        using var context = Factory.CreateContext<GamesDbContext>();
        var stored = context.Quizzes.Single(candidate => candidate.Id == quiz.Id);
        stored.Status.Should().Be(QuizStatus.Draft);
        stored.Name.Should().Be(quiz.Name);
        stored.Instructions.Should().Be(quiz.Instructions);
        stored.ExperiencePoints.Should().Be(quiz.ExperiencePoints);
        stored.Questions.Should().BeEquivalentTo(quiz.Questions);
    }

    [Theory]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Retired)]
    public async Task A_published_or_retired_quiz_cannot_be_deleted(QuizStatus status)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = GetSeed(status);

        var response = await client.DeleteAsync($"{Url}/{quiz.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        problem!.Title.Should().Contain("draft or archived");
        using var context = Factory.CreateContext<GamesDbContext>();
        context.Quizzes.Single(stored => stored.Id == quiz.Id).Status.Should().Be(status);
    }

    [Theory]
    [InlineData("retire")]
    [InlineData("archive")]
    public async Task A_draft_quiz_cannot_be_retired_or_archived(string command)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = QuizSeed.DraftMuseum;

        var response = await client.PostAsync($"{Url}/{quiz.Id}/{command}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.Quizzes.Single(stored => stored.Id == quiz.Id).Status.Should().Be(QuizStatus.Draft);
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("retire")]
    [InlineData("archive")]
    [InlineData("delete")]
    public async Task A_missing_quiz_cannot_be_managed(string command)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var id = Guid.NewGuid();

        var response = await SendCommand(client, id, command);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("publish", false, HttpStatusCode.Unauthorized)]
    [InlineData("retire", false, HttpStatusCode.Unauthorized)]
    [InlineData("archive", false, HttpStatusCode.Unauthorized)]
    [InlineData("delete", false, HttpStatusCode.Unauthorized)]
    [InlineData("publish", true, HttpStatusCode.Forbidden)]
    [InlineData("retire", true, HttpStatusCode.Forbidden)]
    [InlineData("archive", true, HttpStatusCode.Forbidden)]
    [InlineData("delete", true, HttpStatusCode.Forbidden)]
    public async Task Only_an_administrator_can_manage_quiz_lifecycle(
        string command, bool signedIn, HttpStatusCode expectedStatus)
    {
        var client = signedIn ? Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer") : Factory.CreateClient();

        var response = await SendCommand(client, QuizSeed.DraftMuseum.Id, command);

        response.StatusCode.Should().Be(expectedStatus);
    }

    private static Quiz GetSeed(QuizStatus status)
    {
        return status switch
        {
            QuizStatus.Draft => QuizSeed.DraftMuseum,
            QuizStatus.Published => QuizSeed.PublishedSquare,
            QuizStatus.Retired => QuizSeed.RetiredPark,
            QuizStatus.Archived => QuizSeed.ArchivedBridge,
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
    }

    private static Task<HttpResponseMessage> SendCommand(HttpClient client, Guid id, string command)
    {
        return command == "delete" ? client.DeleteAsync($"{Url}/{id}") : client.PostAsync($"{Url}/{id}/{command}", null);
    }
}
