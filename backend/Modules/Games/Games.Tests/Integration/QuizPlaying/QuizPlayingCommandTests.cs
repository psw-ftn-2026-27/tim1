using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizPlaying;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizPlaying;

public sealed class QuizPlayingCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/games/play/quizzes";

    public QuizPlayingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Player_near_the_quiz_activates_it_and_an_attempt_with_its_start_moment_appears()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedLandmarkWalk;
        var requestedAt = DateTime.UtcNow;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsJsonAsync($"{Url}/{quiz.Id}/activate",
            new ActivateQuizDto(quiz.Latitude!.Value, quiz.Longitude!.Value));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore + 1);
        var attempt = assertContext.QuizAttempts.Single(attempt =>
            attempt.PlayerId == WellKnownUsers.Explorer && attempt.QuizId == quiz.Id);
        attempt.StartedAt.Should().BeOnOrAfter(requestedAt.AddSeconds(-1));
        attempt.CompletedAt.Should().BeNull();
        attempt.Answers.Should().BeEmpty();
    }

    [Fact]
    public async Task Last_answer_completes_the_attempt_and_awards_experience_points_in_proportion_to_correct_answers()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedFountainTrail;
        var lastQuestion = quiz.Questions[1];
        var requestedAt = DateTime.UtcNow;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var completionCountBefore = arrangeContext.ChallengeCompletions.Count();

        var response = await client.PostAsJsonAsync($"{Url}/{quiz.Id}/answers", new AnswerQuestionDto(lastQuestion.Id, 0));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var attempt = assertContext.QuizAttempts.Single(attempt =>
            attempt.PlayerId == WellKnownUsers.Explorer && attempt.QuizId == quiz.Id);
        attempt.CompletedAt.Should().NotBeNull();
        attempt.Answers.Should().HaveCount(2);
        var lastAnswer = attempt.Answers.Single(answer => answer.QuestionId == lastQuestion.Id);
        lastAnswer.AnswerIndex.Should().Be(0);
        lastAnswer.IsCorrect.Should().BeFalse();
        lastAnswer.AnsweredAt.Should().BeOnOrAfter(requestedAt.AddSeconds(-1));
        assertContext.ChallengeCompletions.Count().Should().Be(completionCountBefore + 1);
        var completion = assertContext.ChallengeCompletions.Single(completion =>
            completion.PlayerId == WellKnownUsers.Explorer && completion.ChallengeId == quiz.Id);
        completion.ExperiencePointsAwarded.Should().Be(13);
    }

    [Fact]
    public async Task Player_too_far_from_the_quiz_gets_no_attempt()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedLandmarkWalk;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsJsonAsync($"{Url}/{quiz.Id}/activate", new ActivateQuizDto(45.26, 19.85));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
        assertContext.QuizAttempts.Any(attempt =>
            attempt.PlayerId == WellKnownUsers.Explorer && attempt.QuizId == quiz.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Activating_a_quiz_that_does_not_exist_is_not_found_and_nothing_changes()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsJsonAsync($"{Url}/{Guid.NewGuid()}/activate", new ActivateQuizDto(45, 19));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
    }

    [Fact]
    public async Task Answering_a_quiz_that_does_not_exist_is_not_found_and_nothing_changes()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var completionCountBefore = arrangeContext.ChallengeCompletions.Count();
        var answerCountBefore = arrangeContext.QuizAttempts.Sum(attempt => attempt.Answers.Count);

        var response = await client.PostAsJsonAsync($"{Url}/{Guid.NewGuid()}/answers",
            new AnswerQuestionDto(Guid.NewGuid(), 0));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.ChallengeCompletions.Count().Should().Be(completionCountBefore);
        assertContext.QuizAttempts.Sum(attempt => attempt.Answers.Count).Should().Be(answerCountBefore);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task Only_an_explorer_can_activate_a_quiz(bool signedIn, HttpStatusCode expectedStatus)
    {
        using var client = signedIn
            ? Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator")
            : Factory.CreateClient();
        var quiz = QuizSeed.PublishedLandmarkWalk;

        var response = await client.PostAsJsonAsync($"{Url}/{quiz.Id}/activate",
            new ActivateQuizDto(quiz.Latitude!.Value, quiz.Longitude!.Value));

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task Only_an_explorer_can_answer_a_quiz_question(bool signedIn, HttpStatusCode expectedStatus)
    {
        using var client = signedIn
            ? Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator")
            : Factory.CreateClient();
        var quiz = QuizSeed.PublishedFountainTrail;

        var response = await client.PostAsJsonAsync($"{Url}/{quiz.Id}/answers",
            new AnswerQuestionDto(quiz.Questions[1].Id, 0));

        response.StatusCode.Should().Be(expectedStatus);
    }
}
