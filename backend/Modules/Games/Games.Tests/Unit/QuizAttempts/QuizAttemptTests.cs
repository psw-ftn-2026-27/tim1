using FluentAssertions;
using Games.Domain.QuizAttempts;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.QuizAttempts;

public sealed class QuizAttemptTests
{
    private static readonly DateTime Now = new(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Points_are_the_number_of_correct_answers()
    {
        var attempt = new QuizAttempt(Guid.NewGuid(), Guid.NewGuid(), Now);
        attempt.AddAnswer(new GivenAnswer(Guid.NewGuid(), 0, true, Now));
        attempt.AddAnswer(new GivenAnswer(Guid.NewGuid(), 1, false, Now));
        attempt.AddAnswer(new GivenAnswer(Guid.NewGuid(), 0, true, Now));

        var points = attempt.Points;

        points.Should().Be(2);
    }

    [Fact]
    public void A_completed_attempt_takes_no_more_answers()
    {
        var attempt = new QuizAttempt(Guid.NewGuid(), Guid.NewGuid(), Now);
        attempt.Complete(Now);
        var answer = () => attempt.AddAnswer(new GivenAnswer(Guid.NewGuid(), 0, true, Now));

        answer.Should().Throw<DomainException>();
        attempt.Answers.Should().BeEmpty();
    }

    [Fact]
    public void A_question_cannot_be_answered_twice()
    {
        var questionId = Guid.NewGuid();
        var attempt = new QuizAttempt(Guid.NewGuid(), Guid.NewGuid(), Now);
        attempt.AddAnswer(new GivenAnswer(questionId, 1, false, Now));
        var answerAgain = () => attempt.AddAnswer(new GivenAnswer(questionId, 0, true, Now));

        answerAgain.Should().Throw<DomainException>();
        attempt.Answers.Should().ContainSingle().Which.AnswerIndex.Should().Be(1);
        attempt.Points.Should().Be(0);
    }

    [Fact]
    public void Completing_an_attempt_records_the_moment_of_completion()
    {
        var attempt = new QuizAttempt(Guid.NewGuid(), Guid.NewGuid(), Now);
        var completedAt = Now.AddMinutes(5);

        attempt.Complete(completedAt);

        attempt.CompletedAt.Should().Be(completedAt);
    }
}
