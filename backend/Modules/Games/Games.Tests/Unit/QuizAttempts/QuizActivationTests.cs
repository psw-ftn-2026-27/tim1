using FluentAssertions;
using Games.Domain.ChallengeCompletions;
using Games.Domain.GeoPositions;
using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.QuizAttempts;

public sealed class QuizActivationTests
{
    private static readonly DateTime Now = new(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Player = Guid.NewGuid();
    private static readonly GeoPosition AtTheQuiz = new(45, 19);

    [Fact]
    public void A_player_within_reach_of_a_published_quiz_starts_an_attempt()
    {
        var quiz = CreatePublishedQuiz();
        var activation = new QuizActivation();

        var attempt = activation.Activate(Player, quiz, AtTheQuiz, null, null, Now);

        attempt.Should().NotBeNull();
        attempt!.PlayerId.Should().Be(Player);
        attempt.QuizId.Should().Be(quiz.Id);
        attempt.StartedAt.Should().Be(Now);
        attempt.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void A_player_too_far_from_the_quiz_cannot_start_an_attempt()
    {
        var quiz = CreatePublishedQuiz();
        var activation = new QuizActivation();
        var activate = () => activation.Activate(Player, quiz, new GeoPosition(45.01, 19), null, null, Now);

        activate.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_player_who_already_solved_the_quiz_cannot_start_it_again()
    {
        var quiz = CreatePublishedQuiz();
        var completion = new ChallengeCompletion(Player, quiz.Id, ChallengeType.Quiz, Now, 10);
        var activation = new QuizActivation();
        var activate = () => activation.Activate(Player, quiz, AtTheQuiz, null, completion, Now);

        activate.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Retired)]
    [InlineData(QuizStatus.Archived)]
    public void A_quiz_that_is_not_published_cannot_be_started(QuizStatus status)
    {
        var quiz = CreateQuizInState(status);
        var activation = new QuizActivation();
        var activate = () => activation.Activate(Player, quiz, AtTheQuiz, null, null, Now);

        activate.Should().Throw<DomainException>();
    }

    [Fact]
    public void An_attempt_in_progress_is_continued_without_a_new_attempt_even_far_from_the_quiz()
    {
        var quiz = CreatePublishedQuiz();
        var inProgress = new QuizAttempt(Player, quiz.Id, Now);
        var activation = new QuizActivation();

        var attempt = activation.Activate(Player, quiz, new GeoPosition(-33, 151), inProgress, null, Now.AddHours(1));

        attempt.Should().BeNull();
    }

    private static Quiz CreatePublishedQuiz() => CreateQuizInState(QuizStatus.Published);

    private static Quiz CreateQuizInState(QuizStatus status)
    {
        var quiz = new Quiz("City walk", "Read the signs.", 10,
            [new QuizQuestion("Which sign?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)])], 45, 19);
        if (status != QuizStatus.Draft)
        {
            quiz.Publish();
        }
        if (status is QuizStatus.Retired or QuizStatus.Archived)
        {
            quiz.Retire();
        }
        if (status == QuizStatus.Archived)
        {
            quiz.Archive();
        }

        return quiz;
    }
}
