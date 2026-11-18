using FluentAssertions;
using Games.Domain.ChallengeCompletions;
using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.QuizAttempts;

public sealed class QuizAnsweringTests
{
    private static readonly DateTime Now = new(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Player = Guid.NewGuid();

    [Fact]
    public void An_answer_to_the_current_question_is_stored_with_its_correctness_and_moment()
    {
        var quiz = CreatePublishedQuiz();
        var attempt = new QuizAttempt(Player, quiz.Id, Now);
        var answering = new QuizAnswering();
        var first = quiz.Questions[0];

        var completion = answering.Answer(attempt, quiz, first.Id, 1, Now.AddMinutes(1));

        completion.Should().BeNull();
        attempt.CompletedAt.Should().BeNull();
        attempt.Answers.Should().ContainSingle()
            .Which.Should().Be(new GivenAnswer(first.Id, 1, false, Now.AddMinutes(1)));
    }

    [Fact]
    public void An_answer_to_a_skipped_question_is_rejected()
    {
        var quiz = CreatePublishedQuiz();
        var attempt = new QuizAttempt(Player, quiz.Id, Now);
        var answering = new QuizAnswering();
        var answer = () => answering.Answer(attempt, quiz, quiz.Questions[1].Id, 0, Now);

        answer.Should().Throw<DomainException>();
        attempt.Answers.Should().BeEmpty();
    }

    [Fact]
    public void An_answer_to_an_already_answered_question_is_rejected_and_the_first_answer_stays()
    {
        var quiz = CreatePublishedQuiz();
        var attempt = new QuizAttempt(Player, quiz.Id, Now);
        var answering = new QuizAnswering();
        answering.Answer(attempt, quiz, quiz.Questions[0].Id, 1, Now);
        var answerAgain = () => answering.Answer(attempt, quiz, quiz.Questions[0].Id, 0, Now);

        answerAgain.Should().Throw<DomainException>();
        attempt.Answers.Should().ContainSingle().Which.AnswerIndex.Should().Be(1);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void An_answer_that_is_not_among_the_offered_answers_is_rejected(int answerIndex)
    {
        var quiz = CreatePublishedQuiz();
        var attempt = new QuizAttempt(Player, quiz.Id, Now);
        var answering = new QuizAnswering();
        var answer = () => answering.Answer(attempt, quiz, quiz.Questions[0].Id, answerIndex, Now);

        answer.Should().Throw<DomainException>();
        attempt.Answers.Should().BeEmpty();
    }

    [Fact]
    public void An_answer_to_a_quiz_that_is_no_longer_published_is_rejected_and_the_attempt_stays_open()
    {
        var quiz = CreatePublishedQuiz();
        var attempt = new QuizAttempt(Player, quiz.Id, Now);
        var answering = new QuizAnswering();
        quiz.Retire();
        var answer = () => answering.Answer(attempt, quiz, quiz.Questions[0].Id, 0, Now);

        answer.Should().Throw<DomainException>();
        attempt.Answers.Should().BeEmpty();
        attempt.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void The_last_answer_completes_the_attempt_and_awards_experience_points_for_the_correct_answers()
    {
        var quiz = CreatePublishedQuiz();
        var attempt = new QuizAttempt(Player, quiz.Id, Now);
        var answering = new QuizAnswering();
        answering.Answer(attempt, quiz, quiz.Questions[0].Id, 0, Now);
        var finishedAt = Now.AddMinutes(2);

        var completion = answering.Answer(attempt, quiz, quiz.Questions[1].Id, 1, finishedAt);

        attempt.CompletedAt.Should().Be(finishedAt);
        completion.Should().NotBeNull();
        completion!.PlayerId.Should().Be(Player);
        completion.ChallengeId.Should().Be(quiz.Id);
        completion.ChallengeType.Should().Be(ChallengeType.Quiz);
        completion.CompletedAt.Should().Be(finishedAt);
        completion.ExperiencePointsAwarded.Should().Be(5);
    }

    private static Quiz CreatePublishedQuiz()
    {
        var quiz = new Quiz("City walk", "Read the signs.", 10,
        [
            new QuizQuestion("Which sign?", [new QuizAnswer("First", true), new QuizAnswer("Second", false)]),
            new QuizQuestion("Which gate?", [new QuizAnswer("North", true), new QuizAnswer("South", false)])
        ]);
        quiz.Publish();

        return quiz;
    }
}
