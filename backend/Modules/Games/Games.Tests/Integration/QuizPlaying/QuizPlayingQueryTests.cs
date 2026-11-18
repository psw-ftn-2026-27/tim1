using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Games.Application.QuizPlaying;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizPlaying;

public sealed class QuizPlayingQueryTests : BaseIntegrationTest
{
    private const string Url = "/api/games/play/quizzes";

    public QuizPlayingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Available_quizzes_hold_an_unsolved_published_quiz_and_not_a_solved_one()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var result = await client.GetFromJsonAsync<PageResult<AvailableQuizDto>>(Url, JsonOptions);

        var unsolved = QuizSeed.PublishedLandmarkWalk;
        var inProgress = QuizSeed.PublishedFountainTrail;
        result!.Items.Should().ContainSingle(quiz => quiz.Id == unsolved.Id)
            .Which.Should().Be(new AvailableQuizDto(unsolved.Id, unsolved.Name, unsolved.ExperiencePoints, true, false));
        result.Items.Should().ContainSingle(quiz => quiz.Id == inProgress.Id)
            .Which.Should().Be(new AvailableQuizDto(inProgress.Id, inProgress.Name, inProgress.ExperiencePoints, false, true));
        result.Items.Should().NotContain(quiz => quiz.Id == QuizSeed.PublishedMonumentTrail.Id);
        result.Items.Should().NotContain(quiz => quiz.Id == QuizSeed.RetiredPark.Id);
        result.Items.Should().NotContain(quiz => quiz.Id == QuizSeed.DraftMuseum.Id);
    }

    [Fact]
    public async Task Quiz_in_progress_shows_the_first_unanswered_question_without_which_answer_is_correct()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedFountainTrail;

        var body = await client.GetStringAsync($"{Url}/{quiz.Id}");

        body.Should().NotContain("isCorrect");
        var state = JsonSerializer.Deserialize<QuizPlayStateDto>(body, JsonOptions)!;
        state.Kind.Should().Be(QuizPlayKind.InProgress);
        state.Question.Should().BeEquivalentTo(new QuizPlayQuestionDto(quiz.Questions[1].Id, "Which statue?", ["First", "Second"]));
        state.QuestionNumber.Should().Be(2);
        state.QuestionCount.Should().Be(2);
        state.Points.Should().Be(1);
        state.LastAnswerWasCorrect.Should().BeTrue();
    }

    [Fact]
    public async Task Retired_quiz_is_unavailable()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var state = await client.GetFromJsonAsync<QuizPlayStateDto>($"{Url}/{QuizSeed.RetiredPark.Id}", JsonOptions);

        state!.Kind.Should().Be(QuizPlayKind.Unavailable);
        state.QuizId.Should().Be(QuizSeed.RetiredPark.Id);
        state.Question.Should().BeNull();
    }

    [Fact]
    public async Task Solved_quiz_shows_the_percent_of_correct_answers_and_the_awarded_experience_points()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var state = await client.GetFromJsonAsync<QuizPlayStateDto>(
            $"{Url}/{QuizSeed.PublishedMonumentTrail.Id}", JsonOptions);

        state!.Kind.Should().Be(QuizPlayKind.Completed);
        state.CorrectAnswers.Should().Be(1);
        state.QuestionCount.Should().Be(2);
        state.Percent.Should().Be(50);
        state.ExperiencePointsAwarded.Should().Be(ChallengeCompletionSeed.ExplorerSolvedMonumentTrail.ExperiencePointsAwarded);
        state.LastAnswerWasCorrect.Should().BeFalse();
        state.Question.Should().BeNull();
    }
}
